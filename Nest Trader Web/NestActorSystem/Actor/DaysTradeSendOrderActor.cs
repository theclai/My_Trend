using Akka.Actor;
using Nest_Trader_Web.NestActorSystem.Messages;

namespace Nest_Trader_Web.NestActorSystem.Actor
{
    public class DaysTradeSendOrderActor : ReceiveActor
    {
        public DaysTradeSendOrderActor()
        {
            Receive<DaysTradeSendOrderMessage>(message => OnDaysTradeSendOrderMessage(message.AutoExecute, message.RequireSelected));
        }


        private void OnDaysTradeSendOrderMessage(bool autoExecute, bool requireSelected )
        {
            Nest_Trader_Form.Nest_Trader_Form_Instance.Days_Trade_Send_Order(autoExecute, requireSelected);
        }
    }
}
