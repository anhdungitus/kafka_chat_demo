using Confluent.Kafka;
using Confluent.Kafka.Admin;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace ChattingClient
{
    public static class ThreadHelperClass
    {
        delegate void SetTextCallback(Form f, Control ctrl, string text);
        /// <summary>
        /// Set text property of various controls
        /// </summary>
        /// <param name="form">The calling form</param>
        /// <param name="ctrl"></param>
        /// <param name="text"></param>
        public static void SetText(Form form, Control ctrl, string text)
        {
            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 
            if (ctrl.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                form.Invoke(d, new object[] { form, ctrl, text });
            }
            else
            {
                ctrl.Text += text;
            }
        }
    }
    public partial class Form1 : Form
    {
        public bool IsConnect { get; set; } = false;
        public int CurrentUserId;
        public int CurrentChatPartnerId { get; set; }
        public static string ChatTopic { get; set; }
        public const string BootstrapServers = "127.0.0.1:9092";
        private IConsumer<Ignore, string> consumer;

        public void DoWork()
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = BootstrapServers,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                ClientId = CurrentUserId.ToString(), 
                GroupId = CurrentUserId.ToString()
            };
            using (IConsumer<Ignore, string> consumer = new ConsumerBuilder<Ignore, string>(config).Build())
            {
                consumer.Subscribe(ChatTopic);

                while (true)
                {
                    var consumeResult = consumer.Consume();

                    try
                    {
                        var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<Domain.Message>(consumeResult.Message.Value);
                        if (obj.SenderId == CurrentUserId)
                            continue;
                        ThreadHelperClass.SetText(this, chatbox, "===>>>" + obj.Content + obj.SendTime + Environment.NewLine);
                    }
                    catch (Exception e)
                    {
                    }
                }
                consumer.Close();
            }
        }

        public Form1(int currentUserIdId)
        {
            CurrentUserId = currentUserIdId;
            InitializeComponent();
            InitUserList();
        }

        private void InitUserList()
        {
            using (var db = new ChatDbContext())
            {
                var users = db.Users.AsNoTracking().Where(s => s.Id != CurrentUserId).ToList();
                listBox1.Items.AddRange(users.Select(s => s.Name).ToArray());
            }
        }

        private void Send_Click(object sender, EventArgs e)
        {
            if (IsConnect)
            {
                chatbox.Text += richTextBox1.Text + Environment.NewLine;

                var config = new ProducerConfig
                {
                    BootstrapServers = BootstrapServers,
                    ClientId = Dns.GetHostName()
                };

                using (var producer = new ProducerBuilder<Null, string>(config).Build())
                {
                    var message = new ChattingClient.Domain.Message()
                    {
                        SenderId = CurrentUserId,
                        ReceiverId = CurrentChatPartnerId,
                        Content = richTextBox1.Text,
                        SendTime = DateTime.Now
                    };
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(message);
                    producer.ProduceAsync(ChatTopic, new Message<Null, string> { Value = json });
                }
                richTextBox1.Text = String.Empty;
            }
            else
            {
                // Create chat topic chat_userId1_userId2
                using (var db = new ChatDbContext())
                {
                    var user = db.Users.FirstOrDefault(s => s.Name == listBox1.Text);
                    var topic = CurrentUserId > user.Id ? $"chat_{user.Id}_{CurrentUserId}"
                        : $"chat_{CurrentUserId}_{user.Id}";
                    CurrentChatPartnerId = user.Id;

                    using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = BootstrapServers }).Build())
                    {
                        try
                        {
                            var data = adminClient.GetMetadata(topic, TimeSpan.FromMinutes(1));
                            if (data.Topics.Count != 1)
                            {
                                adminClient.CreateTopicsAsync(new TopicSpecification[]
                                {
                                    new TopicSpecification {Name = topic, ReplicationFactor = 1, NumPartitions = 1}
                                });
                            }
                            ChatTopic = topic;
                        }
                        catch (CreateTopicsException ex)
                        {
                        }
                    }

                    Send.Text = "Send";
                    chatbox.Visible = true;
                    IsConnect = true;
                    chatbox.Text += "Ban da ket noi voi " + listBox1.SelectedItem + Environment.NewLine;

                    Thread consumer = new Thread(DoWork);
                    consumer.Start();
                }
            }
        }
    }
}