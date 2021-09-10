using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Prometheus;
using RestSharp;

namespace OctopusEnergyExporter
{
    class Program
    {
        private static readonly Gauge ElecConsumption =
            Metrics.CreateGauge("elec_consuption", "Amount of electricity consumed in last 30 mins");

        private static readonly Gauge ElecStandingCharge =
            Metrics.CreateGauge("elec_standing_charge", "Cost of electricity per day");

        private static readonly Gauge ElecUnitCharge =
            Metrics.CreateGauge("elec_unit_charge", "Cost of electricity per kWh");

        private static readonly Gauge ElecLastUpdated =
            Metrics.CreateGauge("elec_last_updated", "Timestamp of the latest electricity reading",
                new GaugeConfiguration
                {
                    LabelNames = new[] { "lastupdated" }
                });

        private static readonly Gauge GasConsumption =
            Metrics.CreateGauge("gas_consuption", "Amount of gas consumed in last 30 mins");

        private static readonly Gauge GasStandingCharge =
            Metrics.CreateGauge("gas_standing_charge", "Cost of gas per day");

        private static readonly Gauge GasUnitCharge =
            Metrics.CreateGauge("gas_unit_charge", "Cost of gas per kWh");

        private static readonly Gauge GasLastUpdated =
            Metrics.CreateGauge("gas_last_updated", "Timestamp of the latest gas reading",
                new GaugeConfiguration
                {
                    LabelNames = new[] { "lastupdated" }
                });

        static async Task Main()
        {
            IConfiguration config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

            var elecMeterMPAN = config["ELEC_METER_MPAN"];
            var elecMeterSN = config["ELEC_METER_SERIAL_NO"];
            var gasMeterMPRN = config["GAS_METER_MPRN"];
            var gasMeterSN = config["GAS_METER_SERIAL_NO"];
            var apiKey = config["APIKEY"];
            var elecStandingCharge = config["ELEC_STANDING_CHARGE"];
            var elecUnitCharge = config["ELEC_UNIT_CHARGE"];
            var gasStandingCharge = config["GAS_STANDING_CHARGE"];
            var gasUnitCharge = config["GAS_UNIT_CHARGE"];
            var retry = config["RETRY_SECONDS"];
            var port = config["PORT"];

            var encodedApiKey = EncodeAPIKey(apiKey);

            var portNumber = int.Parse(port);
            var server = new MetricServer(port: portNumber);
            server.Start();

            while (true)
            {
                var latestElecValue = GetConsumption(encodedApiKey, "electricity", elecMeterMPAN, elecMeterSN);
                if (latestElecValue.Result == null)
                {
                    ElecConsumption.Set(0);
                    ElecConsumption.WithLabels("Null").Set(0);
                }
                else
                {
                    ElecConsumption.Set(latestElecValue.Result.consumption);
                    ElecLastUpdated.WithLabels($"{latestElecValue.Result.interval_end}").Set(0);
                }


                var parsedElecStandingCharge = double.Parse(elecStandingCharge);
                ElecStandingCharge.Set(parsedElecStandingCharge);

                var parsedElecUnitCharge = double.Parse(elecUnitCharge);
                ElecUnitCharge.Set(parsedElecUnitCharge);

                var latestGasValue = GetConsumption(encodedApiKey, "gas", gasMeterMPRN, gasMeterSN);
                if (latestGasValue.Result == null)
                {
                    GasConsumption.Set(0);
                    GasLastUpdated.WithLabels("null").Set(0);
                }
                else
                {
                    GasConsumption.Set(latestGasValue.Result.consumption);
                    GasLastUpdated.WithLabels($"{latestGasValue.Result.interval_end}").Set(0);
                }

                var parsedGasStandingCharge = double.Parse(gasStandingCharge);
                GasStandingCharge.Set(parsedGasStandingCharge);

                var parsedGasUnitCharge = double.Parse(gasUnitCharge);
                GasUnitCharge.Set(parsedGasUnitCharge);

                var retrySeconds = int.Parse(retry);
                var retryMilliSeconds = retrySeconds * 1000;
                await Task.Delay(retryMilliSeconds);
            }
        }

        private static string EncodeAPIKey(string apikey)
        {
            if (!apikey.EndsWith(':'))
            {
                apikey = $"{apikey}:";
            }
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(apikey);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        private static async Task<Result> GetConsumption(
                                            string apikey,
                                            string fuel,
                                            string meterNo,
                                            string meterSN)
        {
            if (fuel != "electricity" && fuel != "gas")
            {
                throw new ArgumentException(
                    "Type of fuel must be 'electricity' or 'gas'", nameof(fuel));
            }

            var apiUrlPath = $"/{fuel}-meter-points/{meterNo}/meters/{meterSN}/consumption/?page_size=1";
            var result = await CallApi(apikey, apiUrlPath);
            var output = JsonConvert.DeserializeObject<Root>(result);
            if (output.count == 0) { return null; }
            return output.results[0];
        }

        private static async Task<string> CallApi(string apikey, string apiUrlPath)
        {
            var apiBaseURL = "https://api.octopus.energy/v1";
            var apiURL = $"{apiBaseURL}/{apiUrlPath}";
            var requestClient = new RestClient(apiURL);
            requestClient.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", $"Basic {apikey}");
            var response = await requestClient.ExecuteAsync(request);
            return response.Content;
        }
    }
}