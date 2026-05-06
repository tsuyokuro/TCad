using System;
using System.IO;
using System.Windows.Resources;
using System.Windows;

namespace TCad.Util;

class ResourceLoader
{
    // Text resource の読み込み
    // uriに
    // "/GLUtil/WireFrameShader/VertexShader.vert"
    // の様に指定する
    // VertexShader.vertはプロパティのビルドアクションで
    // リソースを指定する
    public static string LoadString(string uri)
    {
        Uri fileUri = new(uri, UriKind.Relative);

        StreamResourceInfo info = Application.GetResourceStream(fileUri);
        Stream stream = info.Stream;

        using (StreamReader reader = new StreamReader(info.Stream))
        {
            return reader.ReadToEnd();
        }
    }
}
