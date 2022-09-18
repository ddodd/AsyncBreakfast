using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncBreakfast
{
    // These classes are intentionally empty for the purpose of this example. They are simply marker classes for the purpose of demonstration, contain no properties, and serve no other purpose.
    internal class Bacon { }
    internal class Coffee { }
    internal class Egg { }
    internal class Juice { }
    internal class Toast { }

    class Program
    {
        private static float timeToMakeCup = 1;
        private static float timeToCookEgg = 3;
        private static float timeToWarmPan = 1;
        private static float timeToFryBacon = 3;
        private static float timeToFlipBacon = .5f;
        private static float timeToToastBread = 3;
        private static float timeToPourJuice = 1;

        private static async Task Main(string[] args)
        {
            //Console.WriteLine("pouring coffee");
            //Coffee cup = PourCoffee();
            //Console.WriteLine("coffee is ready");

            // the following Tasks gets executed immediately
            var makeCoffeeTask = MakeCoffeeAsync(6);
            var eggsTask = FryEggsAsync(2);
            var baconTask = FryBaconAsync(3);
            var toastTask = MakeToastWithButterAndHoneyAsync(2);

            // if we want the juice task to start at the same time, we can start it here like this
            // var juice = PourJuiceAsync();
            // if we want to wait until bacon, eggs & toast is finished, we 
            // await the PourJuiceAsync after the others have completed (as shown below)

            var breakfastTasks = new List<Task> { makeCoffeeTask, eggsTask, baconTask, toastTask };
            while (breakfastTasks.Count > 0)
            {
                Task finishedTask = await Task.WhenAny(breakfastTasks);
                if (finishedTask == eggsTask)
                {
                    Console.WriteLine("eggs are ready");
                }
                else if (finishedTask == baconTask)
                {
                    Console.WriteLine("bacon is ready");
                }
                else if (finishedTask == toastTask)
                {
                    Console.WriteLine("toast is ready");
                }
                else if (finishedTask == makeCoffeeTask)
                {
                    Console.WriteLine("coffee is ready");
                }
                breakfastTasks.Remove(finishedTask);
            }
            // the following code gets executed immediately
            //await juice;

            // the following code executes after the while loop
            await PourJuiceAsync();
            Console.WriteLine("juice is ready");

            Console.WriteLine("Breakfast is ready!");
        }

        private static async Task DoTask(float timeTocomplete)
        {
            await Task.Delay(GetTime(timeTocomplete));
        }

        private static async Task<Coffee> MakeCoffeeAsync(int cups)
        {
            Console.WriteLine($"Making {cups} cups of cofee");
            await DoTask(timeToMakeCup * cups);
            Console.WriteLine($"Pot of {cups} cups of cofee is done");
            return PourCoffee();
        }

        private static async Task<Juice> PourJuiceAsync()
        {
            Console.WriteLine("Pouring orange juice");
            await DoTask(timeToPourJuice);
            return new Juice();
        }

        private static async Task<Toast> MakeToastWithButterAndHoneyAsync(int number)
        {
            var toast = await ToastBreadAsync(number);
            ApplyButter(toast);
            ApplyHoney(toast);

            return toast;
        }

        private static int GetTime(float seconds) => (int)(seconds * 1000);

        private static void ApplyButter(Toast t) => Console.WriteLine("Putting butter on the toast");
        private static void ApplyHoney(Toast t) => Console.WriteLine("Putting honey on the toast");

        private static async Task<Toast> ToastBreadAsync(int slices)
        {
            for (int slice = 0; slice < slices; slice++)
            {
                Console.WriteLine("Putting a slice of bread in the toaster");
            }
            Console.WriteLine("Start toasting...");
            await DoTask(timeToToastBread);

            Console.WriteLine("Remove toast from toaster");

            return new Toast();
        }

        private static async Task<Bacon> FryBaconAsync(int slices)
        {
            Console.WriteLine($"putting {slices} slices of bacon in the pan");
            Console.WriteLine("cooking first side of bacon...");
            await DoTask(timeToFryBacon);

            await FlipBacon(slices);
            Console.WriteLine("cooking the second side of bacon...");
            await DoTask(timeToFryBacon);

            Console.WriteLine("Put bacon on plate");

            return new Bacon();
        }

        private static async Task FlipBacon(int slices)
        {
            for (int slice = 0; slice < slices; slice++)
            {
                Console.WriteLine($"flipping slice of bacon {slice + 1}");
                await DoTask(timeToFlipBacon);
            }
        }

        private static async Task<Egg> FryEggsAsync(int howMany)
        {
            Console.WriteLine("Warming the egg pan...");
            await DoTask(timeToWarmPan);
            Console.WriteLine($"cracking {howMany} eggs");
            Console.WriteLine("cooking the eggs ...");
            await DoTask(timeToCookEgg);
            Console.WriteLine("flipping the eggs ...");
            await DoTask(timeToCookEgg);
            Console.WriteLine("Put eggs on plate");

            return new Egg();
        }

        private static Coffee PourCoffee()
        {
            Console.WriteLine("Pouring coffee");
            return new Coffee();
        }

        private static Juice PourJuiceSynchronously()
        {
            Console.WriteLine("Pouring orange juice");
            return new Juice();
        }


    }
}