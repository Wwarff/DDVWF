using System.IO;
using System.Security.Cryptography;using System.Text;
namespace DDVWF.App;
public sealed class WindowsCredentialStore
{
 readonly string _path;public WindowsCredentialStore(string path)=>_path=path;
 public void Save(string secret){Directory.CreateDirectory(Path.GetDirectoryName(_path)!);var clear=Encoding.UTF8.GetBytes(secret);var protectedBytes=ProtectedData.Protect(clear,null,DataProtectionScope.CurrentUser);File.WriteAllBytes(_path,protectedBytes);}
 public string? Load(){if(!File.Exists(_path))return null;try{return Encoding.UTF8.GetString(ProtectedData.Unprotect(File.ReadAllBytes(_path),null,DataProtectionScope.CurrentUser));}catch(CryptographicException){return null;}}
 public void Clear(){if(File.Exists(_path))File.Delete(_path);}
}
