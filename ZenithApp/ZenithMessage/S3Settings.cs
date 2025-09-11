using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenithApp.ZenithMessage
{
    public class S3Settings
    {
        public string BucketName { get; set; }
        public string AWSBaseURL { get; set; }
        public string AWSRegion { get; set; }
        public string BucketParentKey { get; set; }
        public string BucketChildKey { get; set; }
        public string BucketChildKeyPosts { get; set; }
        public string BucketChildKeyEvents { get; set; }
        public string BucketChildKeyUserProfile { get; set; }
        public string BucketChildKeyMediaFiles { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }

    }
}
