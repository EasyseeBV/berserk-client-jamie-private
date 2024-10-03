extern "C"
{
    long getAvailableDiskSpace () 
    {
            long totalFreeSpace = 0;
            if (@available(iOS 11.0, *)) 
            {
                NSURL *fileURL = [[NSURL alloc] initFileURLWithPath:NSHomeDirectory()];
                NSError *error = nil;
                NSDictionary *results = [fileURL resourceValuesForKeys:@[NSURLVolumeTotalCapacityKey] error:&error];
                if (!results) 
                {
                    NSLog(@"Error Obtaining System Memory Info: Domain = %@, Code = %ld", [error domain], (long)[error code]);
                } 
                else 
                {
                    NSLog(@"Available capacity for important usage: %@", results[NSURLVolumeTotalCapacityKey]);
                    totalFreeSpace = ((NSString *)results[NSURLVolumeTotalCapacityKey]).longLongValue;
                }
            } 
            else 
            {
                NSError *error = nil;
                NSArray *paths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES);
                NSDictionary *dictionary = [[NSFileManager defaultManager] attributesOfFileSystemForPath:[paths lastObject] error: &error];
                
                if (dictionary) 
                {
                    NSNumber *fileSystemSizeInBytes = [dictionary objectForKey: NSFileSystemSize];
                    NSNumber *freeFileSystemSizeInBytes = [dictionary objectForKey:NSFileSystemFreeSize];
                    totalFreeSpace = [freeFileSystemSizeInBytes unsignedLongLongValue];
                } 
                else
                {
                    NSLog(@"Error Obtaining System Memory Info: Domain = %@, Code = %ld", [error domain], (long)[error code]);
                }
                
            }
            return totalFreeSpace;
    }
}