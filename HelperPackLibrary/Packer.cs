using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace HelperPackLibrary
{
    public static class Packer
    {
        private static int maxPriceForProduct =100;

        private static int maxWeightForProduct = 100;

        private static int maxItemCount = 15;

        private static char splitCharItems = '\n';

        private static char splitCharWeighingLimitItem = ':';

        private static char splitCharStartItemValues = '(';

        private static char splitCharEndItemValues = ')';

        private static char splitCharItemDetails = ',';

        private static string splitCharOutputResult = "\n";

        private static string emptyResultString = "-";


        public static string Pack(string filePath)
        {
            return PreparePack(File.ReadAllText(filePath)).SelectedItems;
        }

        /// <summary>
        /// Return PreparePackageOutput which you will get the items with most valuable and with constraints
        /// </summary>
        /// <param name="Input"> WeighingLimit: (ID,Weight,Price) (...) \n</param>
        /// <returns>PreparePackageOutput</returns>
        private static PackOutput PreparePack(string Input)
        {
            PackOutput packOutput = new PackOutput();
            try
            {
                List<PackageInput> ListpackageInput = new List<PackageInput>();

                ListpackageInput = ConvertPackageStringToObject(Input);

                var SelectedItems = SelectItems(ListpackageInput);
                packOutput.SelectedItems = PrepareListForOutput(SelectedItems.ToList());
                packOutput.status = true;
            }
            catch (Exception exception)
            {
                packOutput.status = false;
                packOutput.exeption = exception.ToString();
            }
            return packOutput;
        }

        /// <summary>
        /// It runs SelectItemsForMaxPriceAndLessWeight for each oackageInput
        /// </summary>
        /// <param name="listpackageInput"></param>
        /// <returns>SelectItems</returns>
        private static List<string> SelectItems(List<PackageInput> listpackageInput)
        {
            var SelectedItems = new List<string>();
            foreach (var packageInput in listpackageInput)
            {
                SelectedItems.Add(SelectItemsForMaxPriceAndLessWeight(packageInput.Items, packageInput.WeightLimit));
            }
            return SelectedItems;

        }

        /// <summary>
        /// Changes the Input String to PackageInput object list.
        /// </summary>
        /// <param name="Input">WeighingLimit: (ID,Weight,Price) (...)</param>
        /// <returns></returns>
        private static List<PackageInput> ConvertPackageStringToObject(string Input)
        {
            List<PackageInput> ListpackageInput = new List<PackageInput>();


            var ListItemsWithLimit = Input.Split(splitCharItems);
            foreach (var ItemWithLimit in ListItemsWithLimit)
            {
                PackageInput packageInput = new PackageInput();
                var WeighingLimitAndItems = ItemWithLimit.Split(splitCharWeighingLimitItem);

                packageInput.WeightLimit = Convert.ToDouble(WeighingLimitAndItems[0]);
                var items = WeighingLimitAndItems[1].Replace(" ", "").Split(splitCharStartItemValues, splitCharEndItemValues);
                items = items.Where(x => !string.IsNullOrEmpty(x)).ToArray();

                foreach (var item in items)
                {
                    Item itemToAdd = new Item();

                    var ListitemDetails = item.Split(splitCharItemDetails);
                    itemToAdd.ID = ListitemDetails[0];
                    itemToAdd.Weight = Double.Parse((ListitemDetails[1]), System.Globalization.CultureInfo.InvariantCulture);
                    itemToAdd.Price = Convert.ToDouble(GetDoubleOnly(ListitemDetails[2]));
                    packageInput.Items.Add(itemToAdd);
                }
                ListpackageInput.Add(packageInput);
            }
            return ListpackageInput;

        }

        /// <summary>
        /// Get only the double value, if the text consist of currency etc. strings
        /// </summary>
        /// <param name="DoubleWithString"></param>
        /// <returns></returns>
        private static string GetDoubleOnly(string DoubleWithString)
        {
            return Regex.Match(DoubleWithString, @"\d+\.*\d*").Value;
        }

        /// <summary>
        /// Prepare the Output String with the splitCharOutputResult.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private static string PrepareListForOutput(List<string> list)
        {
            return String.Join(splitCharOutputResult, list);
        }


        /// <summary>
        /// It checks the constraints and return to most pricy/weight allowed item combination 
        /// </summary>
        /// <param name="Items"></param>
        /// <param name="LimitWeight">Limit for every package</param>
        /// <returns></returns>
        private static string SelectItemsForMaxPriceAndLessWeight(List<Item> Items, double LimitWeight)
        {
            var BestOptions = emptyResultString;

            double MaxPrice = 0;
            double MaxPriceWeight = 0;

            CheckConstraints(Items);

            var combinations = GetCombinations(Items).ToArray();

            foreach (var combination in combinations)
            {
                if (combination.Sum(a => a.Weight) <= LimitWeight)
                {
                    if (combination.Sum(a => a.Price) > MaxPrice || (combination.Sum(a => a.Price) == MaxPrice && MaxPriceWeight > combination.Sum(a => a.Weight)))
                    {
                        MaxPrice = combination.Sum(a => a.Price);
                        BestOptions = String.Join(",", combination.Select(a => a.ID));
                        MaxPriceWeight = combination.Sum(a => a.Weight);
                    }
                }
            }

            return BestOptions;
        }

        /// <summary>
        /// Check Constraints that will throw an exaption. Do this before selecting which items to select to save time.
        /// </summary>
        /// <param name="Items"></param>
        private static void CheckConstraints(List<Item> Items)
        {
            if (Items.Where(a => a.Price > maxPriceForProduct).ToList().Count > 0)
            {
                throw new APIException($"Price can not be more than { maxPriceForProduct }" );
            }

            if (Items.Where(a => a.Weight > maxWeightForProduct).ToList().Count > 0)
            {
                throw new APIException($"Weight can not be more than {maxPriceForProduct}");
            }

            if (Items.Count > maxItemCount)
            {
                throw new APIException($"Items can not be more than {maxPriceForProduct }");
            }
        }

        /// <summary>
        /// Gives every possible combination for a list.
        /// </summary>
        /// <typeparam name="T">We use T cause the source can be any list of an object so that we do not have to write our code again.</typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        private static IEnumerable<T[]> GetCombinations<T>(IEnumerable<T> source)
        {
            T[] data = source.ToArray();

            return Enumerable
              .Range(0, 1 << (data.Length))
              .Select(index => data
                 .Where((v, i) => (index & (1 << i)) != 0)
                 .ToArray());
        }
    }
}
