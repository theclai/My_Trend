using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nest_Trader_Web.NestActorSystem.Messages
{
    public class ApiOrderSendMessage
    {

        public string ChartId { get; private set; }
        public string ExchangeName { get; private set; }
        public string OrderSymbol { get; private set; }
        public string InstrumentName { get; private set; }
        public string OrderType { get; private set; }
        public string ProductType { get; private set; }
        public string OrderExpirationDate { get; private set; }
        public string OrderQuantity { get; private set; }
        public string DiscQuantity { get; private set; }
        public string OrderPrice { get; private set; }
        public string OrderBuySell { get; private set; }
        public string OrderTakeProfit { get; private set; }
        public string OrderStopLoss { get; private set; }
        public string OrderTakeProfitPl { get; private set; }
        public string OrderStopLossPl { get; private set; }
        public bool AutoExecute { get; private set; }
        public bool UpdateOnPositionsTab { get; private set; }


        public ApiOrderSendMessage(string chartId, string exchangeName, string orderSymbol, string instrumentName, string orderType,
            string productType, string orderExpirationDate, string orderQuantity, string discQuantity, string orderPrice,
            string orderBuySell, string orderTakeProfit, string orderStopLoss, string orderTakeProfitPl, string orderStopLossPl, 
            bool executeAuto, bool updateOnPositionsTab)
        {
            ChartId = chartId;
            ExchangeName = exchangeName;
            OrderSymbol = orderSymbol;
            InstrumentName = instrumentName;
            OrderType = orderType;
            ProductType = productType;
            OrderExpirationDate = orderExpirationDate;
            OrderQuantity = orderQuantity;
            DiscQuantity = discQuantity;
            OrderPrice = orderPrice;
            OrderBuySell = orderBuySell;
            OrderTakeProfit = orderTakeProfit;
            OrderStopLoss = orderStopLoss;
            OrderTakeProfitPl = orderTakeProfitPl;
            OrderStopLossPl = orderStopLossPl;
            AutoExecute = executeAuto;
            UpdateOnPositionsTab = updateOnPositionsTab;
        }
    }
}
