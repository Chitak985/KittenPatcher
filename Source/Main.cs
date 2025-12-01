using Brutal.Logging;
using KSA;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace Kitten_Patcher
{
  [StarMapMod]
  public class MainKittenPatcher
  {
    public XDocument patchXML; 
    [StarMapBeforeMain]
    public void LoadAndPatch()
    {
      patchXML = XDocument.Load("C:\\Program Files\\Kitten Space Agency\\Content\\Patching.xml");
      if(patchXML.Root.Name.ToString() == "KittenPatch")
      {
        e;
      }
      else
      {
        Console.WriteLine("Root node in Patching.xml is invalid! ("+patchXML.Root.Name.ToString()+")");
      }
    }
  }
}
