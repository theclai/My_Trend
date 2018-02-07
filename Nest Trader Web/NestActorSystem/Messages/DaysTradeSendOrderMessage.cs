namespace Nest_Trader_Web.NestActorSystem.Messages
{
   public class DaysTradeSendOrderMessage
    {
       public DaysTradeSendOrderMessage(bool autoExecute, bool requireSelected)
       {
           AutoExecute = autoExecute;
           RequireSelected = requireSelected;
       }

       public bool AutoExecute { get; private set; }
       public bool RequireSelected { get; private set; }

    }
}
