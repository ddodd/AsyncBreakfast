using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncBreakfast
{
    // These classes are intentionally empty for the purpose of this example. They are simply marker classes for the purpose of demonstration, contain no properties, and serve no other purpose.
    internal class Bacon { }
    internal class Coffee { }
    internal class Eggs { }
    internal class Juice { }
    internal class Toast { }

    class Program
    {
        private static float timeForShortTask = .5f;
        private static float timeToMakeCup = 1;
        private static float timeToCookEgg = 3;
        private static float timeToWarmPan = 1;
        private static float timeToFryBacon = 3;
        private static float timeToToastBread = 3;
        private static float timeToPourJuice = 1;

        private static async Task Main(string[] args)
        {
            await Delay(1);
            Console.WriteLine("Making Breakfast");
            await Delay(1);

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

        private static async Task Delay(float timeTocomplete)
        {
            await Task.Delay(GetTime(timeTocomplete));
        }

        private static async Task<Coffee> MakeCoffeeAsync(int cups)
        {
            Console.WriteLine($"Making {cups} cups of coffee");
            await Delay(timeToMakeCup * cups);
            Console.WriteLine($"Pot of {cups} cups of coffee is done");
            return await PourCoffee();
        }

        private static async Task<Juice> PourJuiceAsync()
        {
            Console.WriteLine("Pouring juice");
            await Delay(timeToPourJuice);
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
            LoadToaster(slices).Wait();
            //await LoadToaster(slices);
            Console.WriteLine("Start toasting...");
            await Delay(timeToToastBread);

            Console.WriteLine("Remove toast from toaster");

            return new Toast();
        }

        private static async Task<Toast> LoadToaster(int slices)
        {
            for (int slice = 0; slice < slices; slice++)
            {
                Console.WriteLine($"Putting slice of bread {slice + 1} in the toaster");
                await Delay(timeForShortTask);
            }
            return new Toast();
        }

        private static async Task<Bacon> FryBaconAsync(int slices)
        {
            Console.WriteLine($"putting {slices} slices of bacon in the pan");
            Console.WriteLine("cooking first side of bacon...");
            await Delay(timeToFryBacon);

            // what I want is to flip all 3 slices of bacon sequentially without getting interrupted by another task, like this:
            /*
            flipping slice of bacon 1
            flipping slice of bacon 2
            flipping the eggs ...
            flipping slice of bacon 3             
             */

            await FlipBacon(slices);

            //FlipBacon(slices).GetAwaiter().GetResult();
            //FlipBacon(slices).Wait();

            Console.WriteLine("cooking the second side of bacon...");
            await Delay(timeToFryBacon);

            Console.WriteLine("Put bacon on plate");

            return new Bacon();
        }

        private static async Task FlipBacon(int slices)
        {
            for (int slice = 0; slice < slices; slice++)
            {
                Console.WriteLine($"flipping slice of bacon {slice + 1}");
                await Delay(timeForShortTask);
            }
        }

        private static async Task<Eggs> FryEggsAsync(int howMany)
        {
            Console.WriteLine("Warming the egg pan...");
            await Delay(timeToWarmPan);
            Console.WriteLine($"cracking {howMany} eggs");
            Console.WriteLine("cooking the eggs ...");
            await Delay(timeToCookEgg);
            Console.WriteLine("flipping the eggs ...");
            await Delay(timeToCookEgg);
            Console.WriteLine("Put eggs on plate");

            return new Eggs();
        }

        private static async Task<Coffee> PourCoffee()
        {
            Console.WriteLine("Pouring coffee");
            await Delay(timeForShortTask);
            return new Coffee();
        }

        private static Juice PourJuiceSynchronously()
        {
            Console.WriteLine("Pouring orange juice");
            return new Juice();
        }
    }

    class MessageQueue
    {
        public List<string> buffer = new List<string>();
        public bool blocked;
        public void Log(string msg)
        {
            if (blocked)
            {
                buffer.Add(msg);
            }
            else
            {
                Console.WriteLine(msg);
            }
        }

        public void Unblock()
        {
            blocked = false;
            buffer.ForEach(Log);
            buffer.Clear();
        }
    }

    /* Ideally, I want to flip all the bacon before flipping the egs
     * Console output:
    Making 6 cups of cofee
    Warming the egg pan...
    putting 3 slices of bacon in the pan
    cooking first side of bacon...
    Putting a slice of bread in the toaster
    Putting a slice of bread in the toaster
    Start toasting...
    cracking 2 eggs
    cooking the eggs ...
    Remove toast from toaster
    Putting butter on the toast
    Putting honey on the toast
    toast is ready
    flipping slice of bacon 1
    flipping slice of bacon 2
    flipping the eggs ...
    flipping slice of bacon 3
    cooking the second side of bacon...
    Pot of 6 cups of cofee is done
    Pouring coffee
    coffee is ready
    Put eggs on plate
    eggs are ready
    Put bacon on plate
    bacon is ready
    Pouring orange juice
    juice is ready
    Breakfast is ready!

    By using FlipBacon(slices).Wait(); we get the desired result
    Making 6 cups of coffee
    Warming the egg pan...
    putting 3 slices of bacon in the pan
    cooking first side of bacon...
    Putting slice of bread 1 in the toaster
    Putting slice of bread 2 in the toaster
    cracking 2 eggs
    cooking the eggs ...
    Start toasting...
    flipping slice of bacon 1
    flipping slice of bacon 2
    flipping slice of bacon 3
    flipping the eggs ...
    Remove toast from toaster
    Putting butter on the toast
    Putting honey on the toast
    toast is ready
    cooking the second side of bacon...
    Pot of 6 cups of coffee is done
    Pouring coffee
    coffee is ready
    Put eggs on plate
    eggs are ready
    Put bacon on plate
    bacon is ready
    Pouring juice
    juice is ready
    Breakfast is ready!

    */
}