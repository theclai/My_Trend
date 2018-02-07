using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Nest_Trader_Web.NestActorSystem.Messages;

namespace Nest_Trader_Web.NestActorSystem.Actor
{
   public class APIOrderSendActor : ReceiveActor
    {
       public APIOrderSendActor()
       {
           Receive<ApiOrderSendMessage>( message=> OnApiOrderSendMessage(message));
       }

       public void OnApiOrderSendMessage(ApiOrderSendMessage message)
       {
            string Order_Ticket = Nest_Trader_Form.Nest_Trader_Class_Instance.API_Order_Send(message.ChartId, message.ExchangeName,
               message.OrderSymbol, message.InstrumentName, message.OrderType, message.ProductType,
               message.OrderExpirationDate, message.OrderQuantity,
               message.DiscQuantity,
               message.OrderPrice, message.OrderBuySell,
               message.OrderTakeProfit,
               message.OrderStopLoss,
               message.OrderTakeProfitPl, message.OrderStopLossPl,
               message.AutoExecute);
            Sender.Tell(Order_Ticket, Self);


        }
    }
}
