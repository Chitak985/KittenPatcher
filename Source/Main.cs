using Brutal.Logging;
using KSA;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using StarMap.API;

namespace KittenPatcher
{
    [StarMapMod]
    public class KittenPatcher
    {
        [StarMapBeforeMain]
        public void LoadAndPatch()
        {
            // Move and load all XML files
            List<XDocument> xmls = new List<XDocument>();
            foreach (string xmlFile in Directory.GetFiles("Content\\", "*.xml", SearchOption.AllDirectories))
            {
                xmls.Add(XDocument.Load(xmlFile));
                return; // move files to a new directory here pls
            }

            // Load all XML files to load patches from (use cache)
            foreach (string patchPath in Directory.GetFiles("Content\\KittenPatcher\\Cache\\", "Patching.xml", SearchOption.AllDirectories))
            {
                XDocument patchXML = XDocument.Load(patchPath);  // Load file as an XML document
                if (patchXML == null)
                {
                    Console.WriteLine("Something is VERY wrong!");  // Realisticlly shouldn't happen
                    continue;
                }
                if (patchXML.Root == null)
                {
                    Console.WriteLine("Patching.xml has no root node!");  // Idk how would happen but whatever
                    continue;
                }
                if (patchXML.Root.Name.ToString() == "Patch")  // Loads up the root node
                {
                    foreach (var patchItem in patchXML.Root.Ancestors("PatchItem"))
                    {
                        if (patchItem == null)
                        {
                            Console.WriteLine("PatchItem is null!");
                            continue;
                        }
                        foreach (var patchFile in patchItem.Descendants("PatchFile"))
                        {
                            if (patchFile == null)
                            {
                                Console.WriteLine("PatchFile is null!");
                                continue;
                            }
                            if (patchFile.Attribute("File") == null)
                            {
                                Console.WriteLine("PatchFile has no File attribute!");
                                continue;
                            }
                            if (File.Exists(Path.Combine("Content\\KittenPatcher\\Cache\\", patchFile.Attribute("File").Value)))
                            {
                                if (patchFile.Attribute("PatchDelete") != null)
                                {
                                    foreach (var patchDelete in patchFile.Descendants("PatchDelete"))
                                    {
                                        foreach (var xmlFile in xmls)
                                        {
                                            return;  // Implement patch delete logic here for each file
                                        }
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

            foreach (var xml in xmls)
            {
                return;  // Write patched XMLs back to disk here
            }
        }
    }
}
