using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace DirectoryDeepDive
{
    public class ConsoleProgressBar
    {
        private const int blockCount = 30;
        private readonly char[] spinner = new[] { '|', '/', '-', '\\' };

        private int current = 0;
        private int total = 0;
        private int spinnerIndex = 0;

        private bool active;
        private Task spinnerTask;

        public void Start(int initialTotal)
        {
            total = initialTotal;
            active = true;

            spinnerTask = Task.Run(async () =>
            {
                while (active)
                {
                    Draw();
                    await Task.Delay(100);
                }
            });
        }

        public void Tick(int amount = 1)
        {
            Interlocked.Add(ref current, amount);
        }

        public void AddToTotal(int amount)
        {
            Interlocked.Add(ref total, amount);
        }

        public void SetTotal(int newTotal)
        {
            Interlocked.Exchange(ref total, newTotal);
        }

        public void Stop()
        {
            active = false;
            spinnerTask?.Wait();
            Draw();
            Console.WriteLine();
        }

        private void Draw()
        {
            int localCurrent = current;
            int localTotal = total == 0 ? 1 : total;

            double progress = (double)localCurrent / localTotal;
            progress = Math.Min(progress, 1);

            int filledBlocks = (int)(progress * blockCount);

            string bar = new string('█', filledBlocks) + new string('░', blockCount - filledBlocks);
            int percent = (int)(progress * 100);

            char spin = spinner[spinnerIndex++ % spinner.Length];

            Console.Write($"\r{spin} [{bar}] {localCurrent}/{localTotal} ({percent}%)");
        }
    }
}
