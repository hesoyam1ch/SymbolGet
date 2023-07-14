using Binance.Net.Clients;
using Bybit.Net;
using Bybit.Net.Clients;
using Kucoin.Net;
using Kucoin.Net.Clients;
using Microsoft.Extensions.Logging;
using Huobi.Net;
using Huobi.Net.Clients;
using Kraken.Net;
using Kraken.Net.Clients;
using Gate.IO.Api;
using System.Net.WebSockets;
using System.Net.Http;
using System;
using Newtonsoft.Json;
using Gate.IO.Api.Models.RestApi.Spot;

namespace SymbolGet
{
    public class Program
    {
         
        static async Task Main(string[] args)
        {
            string WhitebitEndPoint = "/api/v1/public/symbols";

            var WbtList = new List<string>();
            var GateIoList = new List<string>();
            var KrakenList = new List<string>();
            var HuobiList = new List<string>();
            var ByBitList = new List<string>();
            var BinanceList = new List<string>();

            var KucoinSymbolListCorrect = new List<string>();

            var binanceRestClient = new BinanceRestClient(options =>
            {
                options.Environment = Binance.Net.BinanceEnvironment.Live;
            });

            var bybitRestClient = new BybitRestClient(options =>
            {
                options.Environment = BybitEnvironment.Live;
            });

            var kucoinRestClient = new KucoinRestClient(options =>
            {
                options.Environment = KucoinEnvironment.Live;
            });
            var huobiRestClient = new HuobiRestClient(options =>
            {
                options.Environment = HuobiEnvironment.Live;
            });

            var krakenClient = new KrakenRestClient(options =>
            {
                options.Environment = KrakenEnvironment.Live;
            });

            var gateOptions = new GateRestApiClientOptions();
            var gateIoClient = new GateRestApiClient(gateOptions);
            

            var binanceSpotSymbolData = await binanceRestClient.SpotApi.ExchangeData.GetExchangeInfoAsync();

            foreach (var element in binanceSpotSymbolData.Data.Symbols)
            {
                Console.WriteLine(element.Name);
                BinanceList.Add(element.Name);
            }

            var bybitSpotSymbolData = await bybitRestClient.SpotApiV3.ExchangeData.GetSymbolsAsync();

            Console.WriteLine("Bybit Symbol Data");
            foreach (var element in bybitSpotSymbolData.Data)
            {
                Console.WriteLine(element.Name);
                ByBitList.Add(element.Name);
            }

            Console.WriteLine("KuCoin Symbol Data");

            var kucoinSymbolData = await kucoinRestClient.SpotApi.ExchangeData.GetSymbolsAsync();
            foreach (var element in kucoinSymbolData.Data)
            {
               // Console.WriteLine(element.Name);
                var symbolCorrect = element.Name.Replace("-", "");
                KucoinSymbolListCorrect.Add(symbolCorrect);
            }
            Console.WriteLine("Update kucoin");
            foreach (var element in KucoinSymbolListCorrect)
            {
                Console.WriteLine(element);
            }

            Console.WriteLine("huobi");

            var huobiSymbolData = await huobiRestClient.SpotApi.ExchangeData.GetSymbolsAsync();
            foreach(var element in huobiSymbolData.Data)
            {
                Console.WriteLine(element.Name.ToUpper());
                HuobiList.Add(element.Name.ToUpper());
            }

            Console.WriteLine("kraken");
            var krakenSymbolData = await krakenClient.SpotApi.ExchangeData.GetSymbolsAsync();
            foreach (var element in krakenSymbolData.Data)
            {
                Console.WriteLine(element.Value.AlternateName);
                KrakenList.Add(element.Value.AlternateName);
            }

            Console.WriteLine("GateIo");
            var gateIoSymbolData = await gateIoClient.Spot.GetAllPairsAsync();
            var tmp = gateIoSymbolData.Data;
            foreach(var element in tmp)
            {
                element.Symbol.Replace("_", "");
                Console.WriteLine(element.Symbol);
                GateIoList.Add(element.Symbol.Replace("_",""));

            }
            Console.WriteLine("WBT");

            using(HttpClient whiteBitClient = new HttpClient())
            {
                UriBuilder wbtBasic = new UriBuilder("whitebit.com");
               
                whiteBitClient.BaseAddress = wbtBasic.Uri;
                HttpResponseMessage response = await whiteBitClient.GetAsync(WhitebitEndPoint);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseContent);
                    var parsedObject = new {result = new List<string>()};
                    var parsedData = JsonConvert.DeserializeAnonymousType(responseContent, parsedObject);
                    List<string> resultList = parsedData.result;

                    foreach(var element in resultList)
                    {
                        WbtList.Add(element.Replace("_",""));
                    }
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                }
            }

            Console.WriteLine("Wbt token");
            foreach(var element in WbtList)
            {
                Console.WriteLine(element);
            }

            List<string> finishList = BinanceList.Intersect(GateIoList)
                                    .Intersect(ByBitList)
                                    .Intersect(HuobiList)
                                   .Intersect(KucoinSymbolListCorrect)
                                 //  .Intersect(KrakenList)
                                 //   .Intersect(WbtList)
                                    .ToList();

            foreach (var element in finishList)
            {
                Console.WriteLine(element);
                Console.WriteLine(finishList.Count);
            }
            string json = JsonConvert.SerializeObject(finishList, Formatting.Indented);
            string filePath = "D:\\allTokenWithBinanceGateByBitHuobiKucoin.json"; // Замініть шлях на власний

            File.WriteAllText(filePath, json);

            Console.WriteLine("Файл успішно збережено.");


        }
    }
}