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
        [StarMapBeforeMain]
        public void LoadAndPatch()
        {
            // Load all XML files
            List<XDocument> xmls = new List<XDocument>;
            foreach (string xmlFile in Directory.GetFiles("C:\\Program Files\\Kitten Space Agency\\Content\\", "*.xml", SearchOption.AllDirectories))
            {
                xmls.Add(XDocument.Load(xmlFile));
            }

            // Load all XML files to load patches from
            foreach (string patchFile in Directory.GetFiles("C:\\Program Files\\Kitten Space Agency\\Content\\", "Patching.xml", SearchOption.AllDirectories))
            {
                XDocument patchXML = XDocument.Load(patchFile);  // Load file as an XML document
                if(patchXML == null)
                {
                    Console.WriteLine("Something is VERY wrong!");  // Realisticlly shouldn't happen
                    continue;
                }
                if(patchXML.Root == null)
                {
                    Console.WriteLine("Patching.xml has no root node!");  // Idk how would happen but whatever
                    continue;
                }
                if (patchXML.Root.Name.ToString() == "Patch")  // Loads up the root node
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
}
