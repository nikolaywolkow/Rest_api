using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;

/*
 Разработать консольное приложение, которое генерирует список случайных чисел диапозоном от -100 до 100 
в случайном количестве, но не меньше 20 штук и не более 100 , выводит получившуюся последовательность на экран, 
затем следующей строкой выводит отсортированную по одному из алгоритмов сортировки последовательность 
(алгоритм выбирается каждый раз случайным образом).

В приложении должны быть реализованы минимум 2 алгоритма сортировки (на выбор исполнителя). 
Выбор алгоритма сортировки случайный. Результат сортировки отобразить в консоли и реализовать отправку на rest api сервер,
адрес которого берется из файла конфигурации (требуется реализовать только отправку данных,
поднимать сервер и реализовывать на его стороне приём и обработку данных не требуется). 
Исходный код выложить в репозиторий Github и выслать нам его адрес ( сделать публичным ).
*/


namespace Rest_api
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Random r = new Random();
            int size = 20 + r.Next() % 80; // размер от 20 до 100
            int[] array = new int[size];
            for (int i = 0; i < size; i++)
            {
                array[i] = r.Next() % 200 - 100;
                Console.Write(array[i] + " ");
            }
            Console.WriteLine("  ");

            if (r.Next() % 2 == 0)
            {
                Console.WriteLine("QuickSort");
                QuickSort(array);
            }
            else
            {
                Console.WriteLine("MergeSort");
                MergeSort(array);
            }

            for (int i = 0; i < size; i++)
            {
                Console.Write(array[i] + " ");
            }
            Console.WriteLine(" ");
            Post(array);

            Console.ReadKey();
        }
        static public async void Post(int[] array)
        {
            CancellationToken ct = new CancellationToken();
            Program Pr = new Program();
            await Pr.SendRequest(ct, array);

        }
        

        public async Task<string> SendRequest(CancellationToken ct, int[] array)
        {
            string data;
            var baseAddress = new Uri(ConfigurationManager.AppSettings.Get("URL"));
            try
            {
                var content = new FormUrlEncodedContent(new[]
                {
                     new KeyValuePair<string, string>("Value", "["+string.Join(",",array)+"]")
                });

                using (var client = new HttpClient { BaseAddress = baseAddress })
                {
                    var result = await client.PostAsync("", content, ct);
                    var bytes = await result.Content.ReadAsByteArrayAsync();

                    Encoding encoding = Encoding.GetEncoding("utf-8");
                    data = encoding.GetString(bytes, 0, bytes.Length);

                    result.EnsureSuccessStatusCode();
                }
            }
            catch
            {
                data = "Error";
            }
            Console.WriteLine(data);

            return data;
        }


        // sort 1
        static void Merge(int[] array, int lowIndex, int middleIndex, int highIndex)
        {
            var left = lowIndex;
            var right = middleIndex + 1;
            var tempArray = new int[highIndex - lowIndex + 1];
            var index = 0;

            while ((left <= middleIndex) && (right <= highIndex))
            {
                if (array[left] < array[right])
                {
                    tempArray[index] = array[left];
                    left++;
                }
                else
                {
                    tempArray[index] = array[right];
                    right++;
                }

                index++;
            }

            for (var i = left; i <= middleIndex; i++)
            {
                tempArray[index] = array[i];
                index++;
            }

            for (var i = right; i <= highIndex; i++)
            {
                tempArray[index] = array[i];
                index++;
            }

            for (var i = 0; i < tempArray.Length; i++)
            {
                array[lowIndex + i] = tempArray[i];
            }
        }

        static int[] MergeSort(int[] array, int lowIndex, int highIndex)
        {
            if (lowIndex < highIndex)
            {
                var middleIndex = (lowIndex + highIndex) / 2;
                MergeSort(array, lowIndex, middleIndex);
                MergeSort(array, middleIndex + 1, highIndex);
                Merge(array, lowIndex, middleIndex, highIndex);
            }

            return array;
        }

        static int[] MergeSort(int[] array)
        {
            return MergeSort(array, 0, array.Length - 1);
        }



        // sort 2
        static int Partition(int[] array, int minIndex, int maxIndex)
        {
            var pivot = minIndex - 1;
            for (var i = minIndex; i < maxIndex; i++)
            {
                if (array[i] < array[maxIndex])
                {
                    pivot++;
                    Swap(ref array[pivot], ref array[i]);
                }
            }

            pivot++;
            Swap(ref array[pivot], ref array[maxIndex]);
            return pivot;
        }
        static int[] QuickSort(int[] array)
        {
            return QuickSort(array, 0, array.Length - 1);
        }
        static int[] QuickSort(int[] array, int minIndex, int maxIndex)
        {
            if (minIndex >= maxIndex)
            {
                return array;
            }

            var pivotIndex = Partition(array, minIndex, maxIndex);
            QuickSort(array, minIndex, pivotIndex - 1);
            QuickSort(array, pivotIndex + 1, maxIndex);

            return array;
        }
        static void Swap(ref int x, ref int y)
        {
            var t = x;
            x = y;
            y = t;
        }
    }
}