using System.IO.MemoryMappedFiles;

namespace iRacingSDK;

public static class MemoryMappedViewAccessorExtensions
	{
		public unsafe delegate T MyFn<T>(byte* ptr);

		public unsafe static T AcquirePointer<T>(this MemoryMappedViewAccessor self, MyFn<T> fn)
		{
			byte* ptr = null;
			self.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
			try
			{
				return fn(ptr);
			}
			finally
			{
				self.SafeMemoryMappedViewHandle.ReleasePointer();
			}
		}
	}
	
