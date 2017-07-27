using System;

namespace WCFHelper.Compression
{
    internal interface IStiNameTransform
    {
        string TransformFile(string name);
        string TransformDirectory(string name);
    }
}