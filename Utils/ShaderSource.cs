using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fax_cshad.Utils
{

    public enum ShaderType
    {
        Compute,
        Vertex,
        Fragment
    }
    public class ShaderSource
    {
        private readonly string _sourcePath;
        private readonly ShaderType _type;

        public ShaderSource(string sourcePath, ShaderType type)
        {
            _sourcePath = sourcePath;
            _type = type;
        }

        public bool IsValid => File.Exists(_sourcePath);

    }
}
