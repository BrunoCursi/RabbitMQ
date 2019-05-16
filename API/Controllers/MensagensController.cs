using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using API.Models;
using API;

namespace APIMensagens.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MensagensController : ControllerBase
    {
        private static Contador _CONTADOR = new Contador();

        private static string url = "amqp://ktakjvzb:SDqscNMEASELZR_iAxe9iOYgCUwHJmxg@buffalo.rmq.cloudamqp.com/ktakjvzb";
        private static Uri _uri = new Uri(url.Replace("amqp://", "amqps://"));

        static readonly ConnectionFactory factory = new ConnectionFactory
        {
            Uri = _uri
        };

        [HttpGet]
        public object Get()
        {
            return new
            {
                QtdMensagensEnviadas = _CONTADOR.ValorAtual
            };
        }

        [HttpPost]
        public object Post(
            [FromServices]RabbitMQConfigurations configurations,
            [FromBody]Conteudo conteudo)
        {
            lock (_CONTADOR)
            {
                _CONTADOR.Incrementar();

                //var factory = new ConnectionFactory()
                //{
                //    HostName = configurations.HostName,
                //    Port = configurations.Port,
                //    UserName = configurations.UserName,
                //    Password = configurations.Password
                //};
                
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "Mensage",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    string message =
                        $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")} - " +
                        $"Conteúdo da Mensagem: {conteudo.Mensagem}";
                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "",
                                         routingKey: "Mensage",
                                         basicProperties: null,
                                         body: body);
                }

                return new
                {
                    Resultado = "Mensagem encaminhada com sucesso"
                };
            }
        }
    }
}