using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private static float timeToCookEggs = 6;
        private static float timeToWarmPan = 1;
        private static float timeToFryBacon = 3;
        private static float timeToToastBread = 4;
        private static float timeToPourJuice = 1;
        private static MessageQueue report;
        public const string CheckMark = "√";

        private static int numberOfEggs = 2;
        private static int stripsOfBacon = 3;
        private static int piecesOfToast = 2;
        private static int cupsOfCofee = 6;

        private static float timeScale = .25f;
        private static Stopwatch? timer;

        private static async Task Main(string[] args)
        {
            timer = Stopwatch.StartNew();
            report = new MessageQueue(timer);
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            report.Log($"Making Breakfast of {numberOfEggs} eggs, {stripsOfBacon} strips of bacon, {piecesOfToast} pieces of toast");
            report.Log($"and a {cupsOfCofee} cup pot of coffee to boot");
            await Delay(.5f);
            report.Log("☺ wake & bake ...");
            await Delay(1);

            // the following Tasks gets executed immediately
            var makeCoffeeTask = MakeCoffeeAsync(cupsOfCofee);
            var eggsTask = FryEggsAsync(numberOfEggs);
            var baconTask = FryBaconAsync(stripsOfBacon);
            var toastTask = MakeToastWithButterAndHoneyAsync(piecesOfToast);

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
                    report.Log("√ eggs are ready");
                }
                else if (finishedTask == baconTask)
                {
                    report.Log("√ bacon is ready");
                }
                else if (finishedTask == toastTask)
                {
                    report.Log("√ toast is ready");
                }
                else if (finishedTask == makeCoffeeTask)
                {
                    report.Log("√ coffee is ready");
                }
                breakfastTasks.Remove(finishedTask);
            }
            // the following code gets executed immediately
            //await juice;

            // the following code executes after the while loop
            await PourJuiceAsync();
            report.Log("√ juice is ready");

            report.Log("√ ☺ Breakfast is ready! ☺");
        }

        private static async Task Delay(float timeToCompleteInSeconds)
        {
            await Task.Delay(GetTime(timeToCompleteInSeconds));
        }

        private static async Task<Coffee> MakeCoffeeAsync(int cups)
        {
            report.Log($"making {cups} cups of coffee ...");
            await Delay(timeToMakeCup * cups);
            report.Log($"pot of {cups} cups coffee is done");

            return await PourCoffee();
        }

        private static async Task<Coffee> PourCoffee()
        {
            report.Log("Pouring coffee ...");
            await Delay(timeForShortTask);
            var coffee = new Coffee();
            
            return await AddCreamer(coffee);
        }

        private static async Task<Coffee> AddCreamer(Coffee coffee)
        {
            report.Log("adding creamer to coffee ...");
            await Delay(timeForShortTask);
            return coffee;
        }

        private static async Task<Juice> PourJuiceAsync()
        {
            report.Log("pouring fresh juice ...");
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

        private static int GetTime(float seconds) => (int)(seconds * 1000 / timeScale);

        private static void ApplyButter(Toast t) => report.Log("Putting butter on the toast");
        private static void ApplyHoney(Toast t) => report.Log("Putting honey on the toast");

        private static async Task<Toast> ToastBreadAsync(int slices)
        {

            //LoadToaster(slices).Wait();

            report.Block();
            await LoadToaster(slices);
            report.Log("Start toasting ...", true);
            report.Unblock();

            await Delay(timeToToastBread);

            report.Log("Remove toast from toaster");

            return new Toast();
        }

        private static async Task<Toast> LoadToaster(int slices)
        {
            for (int slice = 0; slice < slices; slice++)
            {
                report.Log($"inserting slice of bread {slice + 1} in the toaster", true);
                await Delay(timeForShortTask);
            }
            return new Toast();
        }

        private static async Task<Bacon> FryBaconAsync(int slices)
        {
            report.Log($"putting {slices} slices of bacon in the pan");
            report.Log("cooking first side of bacon ...");
            await Delay(timeToFryBacon);

            // this works but it's not ideal
            // it queues all logs until FlipBacon is complete
            // it requires a special call to force output in awaited task
            report.Block();
            await FlipBacon(slices);
            report.Unblock();

            //FlipBacon(slices).GetAwaiter().GetResult();
            //FlipBacon(slices).Wait();

            report.Log("cooking the second side of bacon ...");
            await Delay(timeToFryBacon);

            report.Log("put bacon on plate");

            return new Bacon();
        }

        private static async Task FlipBacon(int slices)
        {
            for (int slice = 0; slice < slices; slice++)
            {
                report.Log($"flipping slice of bacon {slice + 1}", true);
                await Delay(timeForShortTask);
            }
        }

        private static async Task<Eggs> FryEggsAsync(int howMany)
        {
            report.Log("warming the egg pan ...");
            await Delay(timeToWarmPan);
            report.Log($"cracking {howMany} eggs");
            /* the code inside while loop never runs
            while (!report.IsBlocked)
            {
                report.Block();
                for (int egg = 0; egg < howMany; egg++)
                {
                    report.Log($"cracking egg {egg + 1}", true);
                    await Delay(timeForShortTask);
                }
                report.Unblock();
            }
            */
            report.Log("cooking the eggs ...");
            await Delay(timeToCookEggs/2f);
            report.Log("flipping the eggs ...");
            await Delay(timeToCookEggs/2f);
            report.Log("put eggs on plate");

            return new Eggs();
        }

        private static Juice PourJuiceSynchronously()
        {
            report.Log("Pouring orange juice");
            return new Juice();
        }
    }

    // this works however
    // it only works for one task at a time (see FlipBacon, LoadToaster)
    // it's not scalable
    class MessageQueue
    {
        public MessageQueue(Stopwatch timer)
        {
            this.timer = timer;
        }
        private Stopwatch timer;
        private List<string> buffer = new List<string>();
        private bool blocked;
        private int count = 0;

        public bool IsBlocked => blocked;

        public void Log(string msg)
        {
            if (blocked)
            {
                buffer.Add(msg);
            }
            else
            {
                Write(msg);
            }
        }

        public void Log(string msg, bool force)
        {
            if (force)
            {
                Write(msg);
            }
            else
            {
                Log(msg);
            }
        }

        private void Write(string msg)
        {
            Console.WriteLine($"{++count,2} {timer.Elapsed.TotalSeconds:n4} {msg}");
        }


        public void Block()
        {
            blocked = true;
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