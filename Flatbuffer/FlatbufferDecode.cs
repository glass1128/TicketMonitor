using Collections.Special;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TicketmasterMonitor.Flatbuffer
{
    public class FlatbufferDecode
    {
        static Regex K = new Regex(@"[^+/0-9A-Za-z-_]");
        static int[] encStrLen;
        static int len;
        static Dictionary<int, int> n;

        public async Task<JObject> Decode(string bufferFull)
        {
            JObject jsonData = JObject.Parse(bufferFull);
            string buffer = ConcatEqualSign((string)jsonData["payload"]["data"]["availability"]["buffer"]);

            encStrLen = GetEncStrLength(buffer);
            len = encStrLen[1] > 0 ? encStrLen[0] - 4 : encStrLen[0];

            var charArr = new List<char>();
            n = new Dictionary<int, int>();
            string charS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
            for (int i = 0; i < charS.Length; ++i)
            {
                charArr.Add(charS[i]);
                n[charS[i]] = i;
            }

            byte[] byteArray = ToByteArray(buffer); // flatbuffer binary data

            ByteBuffer bbuffer = new ByteBuffer(byteArray);

            Flatbuffer v1object = new Flatbuffer();
            Flatbuffer flatbuffer_obj = v1object.getRootAsAvailability(bbuffer);

            RoaringBitmapPair SRBP = flatbuffer_obj.statuses(0);
            byte[] t = SRBP.roaringBitmapArray();

            //fetch data
            string url = "https://pubapi.ticketmaster.com/sdk/static/manifest/v1/";
            url += flatbuffer_obj.eventId();

            JObject jsonObject = await FetchDataAsJObject(url);
            JArray placeIdsArray = (JArray)jsonObject["placeIds"];

            //make result json
            JObject resultJson = new JObject();
            resultJson["numGASeats"] = flatbuffer_obj.numGASeats();
            resultJson["numSeats"] = flatbuffer_obj.numSeats();
            resultJson["manifestTimestamp"] = flatbuffer_obj.manifestTimestamp();
            resultJson["pricingTimestamp"] = flatbuffer_obj.pricingTimestamp();
            resultJson["processedTimestamp"] = flatbuffer_obj.processedTimestamp();
            resultJson["revision"] = flatbuffer_obj.revision();
            resultJson["eventId"] = flatbuffer_obj.eventId();
            resultJson["manifestVersion"] = flatbuffer_obj.manifestVersion();
            resultJson["pricingVersion"] = flatbuffer_obj.pricingVersion();
            resultJson["version"] = flatbuffer_obj.version();
            JArray statusesArray = new JArray();

            using (MemoryStream stream = new MemoryStream(t))
            {
                RoaringBitmap roaringBitmap = RoaringBitmap.Deserialize(stream);
                List<int> integerList = new List<int>();

                foreach (int value in roaringBitmap)
                {
                    integerList.Add(value);
                }

                int[] available = integerList.ToArray();

                foreach (int value in available)
                {
                    statusesArray.Add(placeIdsArray[value]);
                }
                resultJson["statuses"] = statusesArray;
            }

            return resultJson;
        }

        static string ConcatEqualSign(string encStr)
        {
            encStr = K.Replace(encStr.Split('=')[0], "");
            int padding = encStr.Length % 4;
            if (padding > 0)
            {
                encStr = encStr.PadRight(encStr.Length + 4 - padding, '=');
            }
            return encStr.Length >= 2 ? encStr : "";
        }

        static int[] GetEncStrLength(string encStr)
        {
            int len = encStr.Length;
            if (len % 4 > 0)
            {
                throw new Exception("Invalid string. Length must be a multiple of 4");
            }

            int pos = encStr.IndexOf("=");
            if (pos == -1)
            {
                pos = len;
            }

            int[] result = new int[2];
            result[0] = pos;
            result[1] = pos == len ? 0 : 4 - (pos % 4);

            return result;
        }

        static int GetValueFromDictionary(Dictionary<int, int> nDic, int key)
        {
            int value;
            if (nDic.TryGetValue(key, out value))
            {
                return value;
            }

            return 0;
        }

        static byte[] ToByteArray(string encStr)
        {
            List<byte> byteArray = new List<byte>();
            int i = 0;

            for (i = 0; i < len; i += 4)
            {
                //int e = (n[encStr[i]] << 18) | (n[encStr[i + 1]] << 12) | (n[encStr[i + 2]] << 6) | n[encStr[i + 3]];
                int e = (GetValueFromDictionary(n, encStr[i]) << 18) | (GetValueFromDictionary(n, encStr[i + 1]) << 12) | (GetValueFromDictionary(n, encStr[i + 2]) << 6) | GetValueFromDictionary(n, encStr[i + 3]);
                byteArray.Add((byte)((e >> 16) & 255));
                byteArray.Add((byte)((e >> 8) & 255));
                byteArray.Add((byte)(255 & e));
            }

            if (encStrLen[1] == 2)
            {
                //int e = (n[encStr[encStr.Length - 2]] << 2) | (n[encStr[encStr.Length - 1]] >> 4);
                int e = (GetValueFromDictionary(n, encStr[i]) << 2) | (GetValueFromDictionary(n, encStr[i + 1]) >> 4);
                byteArray.Add((byte)(255 & e));
            }

            if (encStrLen[1] == 1)
            {
                //int e = (n[encStr[encStr.Length - 1]] << 10) | (n[encStr[encStr.Length - 2]] << 4) | (n[encStr[encStr.Length - 3]] >> 2);
                int e = (GetValueFromDictionary(n, encStr[i]) << 10) | (GetValueFromDictionary(n, encStr[i + 1]) << 4) | (GetValueFromDictionary(n, encStr[i + 2]) >> 2);
                byteArray.Add((byte)((e >> 8) & 255));
                byteArray.Add((byte)(255 & e));
            }

            return byteArray.ToArray();

        }

        static async Task<JObject> FetchDataAsJObject(string apiUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = apiUrl;
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        if (response.Content.Headers.ContentType.MediaType == "application/json")
                        {
                            byte[] compressedData = await response.Content.ReadAsByteArrayAsync();

                            try
                            {
                                using (var decompressedStream = new MemoryStream(compressedData))
                                using (var gzipStream = new GZipStream(decompressedStream, CompressionMode.Decompress))
                                using (var reader = new StreamReader(gzipStream, Encoding.UTF8))
                                {
                                    string decodedContent = reader.ReadToEnd();
                                    JObject jsonObject = JObject.Parse(decodedContent);

                                    return jsonObject;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Error decoding content: " + ex.Message);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Received Content-Type is not 'application/json'.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to retrieve data. Status code: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }

            return null;
        }
    }
}
