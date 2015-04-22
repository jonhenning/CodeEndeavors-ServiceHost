using System;
using System.Diagnostics;
namespace CodeEndeavors.ServiceHost.Common
{
    public class ZipPayload
    {
        private Stopwatch _watch;
        public int StartSize;
        public int EndSize;
        public byte[] Bytes;
        public double CompressionRate;
        public TimeSpan ExecutionTime;
        public ZipPayload(int StartSize)
        {
            this.StartSize = StartSize;
            this._watch = new Stopwatch();
            this._watch.Start();
        }
        public void Complete(byte[] b)
        {
            this._watch.Stop();
            this.ExecutionTime = this._watch.Elapsed;
            this.Bytes = b;
            this.EndSize = b.Length;
            this.CompressionRate = 100.0 - ((this.EndSize < this.StartSize) ? ((double)this.EndSize / (double)this.StartSize) : ((double)this.StartSize / (double)b.Length)) * 100.0;
        }
        public string GetStatistics()
        {
            if (this.StartSize > this.EndSize)
            {
                return string.Format("Compressed {0} bytes down to {1} in {2}ms for a {3}% savings", new object[]
				{
					this.StartSize.ToString("#,##0"),
					this.EndSize.ToString("#,##0"),
					this.ExecutionTime.Milliseconds,
					this.CompressionRate.ToString("##0.0")
				});
            }
            return string.Format("Decompressed {0} bytes up to {1} in {2}ms for a {3}% savings", new object[]
			{
				this.StartSize.ToString("#,##0"),
				this.EndSize.ToString("#,##0"),
				this.ExecutionTime.Milliseconds,
				this.CompressionRate.ToString("##0.0")
			});
        }
    }
}
