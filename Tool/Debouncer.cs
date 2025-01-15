using System;
using System.Threading;
using System.Threading.Tasks;

namespace EquipmentManagment.Tool
{
    public class Debouncer
    {
        private CancellationTokenSource cts;

        /// <summary>
        /// 防抖動
        /// </summary>
        /// <param name="action">動作</param>
        /// <param name="delay">延遲時間(毫秒)</param>
        public void Debounce(Action action, int delay = 300)
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
            }
            cts = new CancellationTokenSource();
            Task.Delay(delay, cts.Token).ContinueWith((t) =>
            {
                if (!t.IsCanceled)
                {
                    action();
                }
            });
        }
    }
}
