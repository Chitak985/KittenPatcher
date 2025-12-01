using Brutal.Logging;
using KSA;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using StarMap.API;

namespace Kitten_Patcher
{
    [StarMapMod]
    public class MainKittenPatcher
    {
        public XDocument patchXML = new XDocument();
        [StarMapBeforeMain]
        public void LoadAndPatch()
        {
            patchXML = XDocument.Load("C:\\Program Files\\Kitten Space Agency\\Content\\Patching.xml");
            if(patchXML == null)
            {
                Console.WriteLine("No Patching.xml file!");
                return;
            }
            if(patchXML.Root == null)
            {
                Console.WriteLine("Patching.xml has no root node!");
                return;
            }
            if (patchXML.Root.Name.ToString() == "KittenPatch")
            {
                foreach (var patchItem in patchXML.Root.Ancestors("PatchItem"))
                {
                    if(patchItem == null)
                    {
                        Console.WriteLine("PatchItem is null!");
                        continue;
                    }
                    foreach (var patchFile in patchItem.Ancestors("PatchFile"))
                    {
                        if(patchFile == null)
                        {
                            Console.WriteLine("PatchFile is null!");
                            continue;
                        }
                        if(patchFile.Attribute("File") == null)
                        {
                            Console.WriteLine("PatchFile has no File attribute!");
                            continue;
                        }
                        if (File.Exists(Path.Combine("C:\\Program Files\\Kitten Space Agency\\Content\\", patchFile.Attribute("File").Value)))
                        {
                            if (patchFile.Attribute("PatchDelete") != null)
                            {
                                foreach (var patchDelete in patchFile.Ancestors("PatchDelete"))
                                {
                                    return;  // Delete stuff here
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Patch file not found: " + patchFile.Attribute("File").Value);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Root node in Patching.xml is invalid! (" + patchXML.Root.Name.ToString() + ")");
            }
        }
    }
}
