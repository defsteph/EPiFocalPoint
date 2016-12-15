using System;
using System.Collections.Generic;
using System.IO;

namespace ImageResizer.Plugins.EPiFocalPoint.Tests.Benchmarks.Internal.Services {
	public class BenchmarkFixture : IDisposable {
		private const string DirectoryPath = "..\\..\\TestData\\Benchmarks";
		private readonly DirectoryInfo directory;
		public BenchmarkFixture() {
			directory = new DirectoryInfo(DirectoryPath);
		}
		public IEnumerable<FileInfo> Files => directory.GetFiles();
		public void Dispose() { }
	}
}