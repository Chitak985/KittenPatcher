using Brutal.Logging;
using KSA;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using StarMap.API;
using System.ComponentModel.DataAnnotations;

namespace KittenPatcher
{
    // Logger class
    public class KittenPatcherLogger
    {
        public void Init()  // Call right after creating a class instance
        {
            if (Directory.Exists("Content\\KittenPatcher\\Logs"))
            {
                Directory.Delete("Content\\KittenPatcher\\Logs", true);
            }
            Directory.CreateDirectory("Content\\KittenPatcher\\Logs");
            File.WriteAllText("Content\\KittenPatcher\\Logs\\MainKittenPatcherLog.log", "KittenPatcher logger started!");
        }
        public void Info(string text)  // Logs a [LOG] text
        {
            File.AppendAllText("Content\\KittenPatcher\\Logs\\MainKittenPatcherLog.log", "\n[LOG] " + text);
        }
        public void Error(string text)  // Logs a [ERR] text
        {
            File.AppendAllText("Content\\KittenPatcher\\Logs\\MainKittenPatcherLog.log", "\n[ERR] " + text);
        }
    }

    // Main class
    [StarMapMod]
    public class KittenPatcher
    {
        public KittenPatcherLogger logging =  new KittenPatcherLogger();
        [StarMapBeforeMain]
        public void LoadAndPatch()
        {
            // ----- SETUP -----

            // Create log category
            logging.Init();
            // The code below does not work so I made my own logger lol
            // LogCategory logging = LogCategory.Make("KittenPatcherLog");

            logging.Info("Beginning loading...");

            // ----- CACHE -----

            // Check cache
            if (Directory.Exists("C:\\KittenPatcherCache"))
            {
                logging.Info("Cache from an old run is found, deleting it...");
                Directory.Delete("C:\\KittenPatcherCache", true);
            }
            else
            {
                logging.Info("Creating Cache folder...");
            }
            Directory.CreateDirectory("C:\\KittenPatcherCache");

            // Move EVERYTHING to Cache
            logging.Info("Moving everything to cache...");
            foreach (string file in Directory.GetFiles("Content\\", "*.*", SearchOption.AllDirectories))
            {
                try
                {
                    Directory.CreateDirectory("C:\\KittenPatcherCache\\" + file.Substring(file.IndexOf("Content\\") + "Content\\".Length, file.LastIndexOf('\\')));
                    File.Copy(file, "C:\\KittenPatcherCache\\" + file.Substring(file.IndexOf("Content\\") + "Content\\".Length), true);
                }
                catch
                {
                    logging.Info("Failed to copy " + file + " to cache, skipping...");
                }
            }

            // Clear up some stuff
            logging.Info("Cleaning up...");
            foreach (string dirs in Directory.GetDirectories("C:\\KittenPatcherCache\\", "*", SearchOption.AllDirectories))
            {
                // This deletes the random empty directories my spaghetti code makes
                try
                {
                    Directory.Delete(dirs);
                }
                catch { }

                // Remove the KittenPatcher directory
                if (dirs.EndsWith("KittenPatcher"))
                {
                    Directory.Delete(dirs, true);
                }
            }
            logging.Info("Cache move finished!");

            // ----- PATCHING -----

            // Load all XML files
            logging.Info("Loading XML files...");
            List<XDocument> xmls = new List<XDocument>();
            foreach (string xmlFile in Directory.GetFiles("Content\\", "*.xml", SearchOption.AllDirectories))
            {
                xmls.Add(XDocument.Load(xmlFile));
            }

            // Load all Patching.xml files to load patches from
            foreach (string patchPath in Directory.GetFiles("Content\\", "Patching.xml", SearchOption.AllDirectories))
            {
                logging.Info("A Patching.xml file was found at "+patchPath+"!");
                XDocument patchXML = XDocument.Load(patchPath);  // Load file as an XML document
                if (patchXML == null)
                {
                    logging.Error("Something is VERY wrong!");  // Realisticlly shouldn't happen
                    continue;
                }
                if (patchXML.Root == null)
                {
                    logging.Error("Patching.xml has no root node!");  // Idk how would happen but whatever
                    continue;
                }
                logging.Info("Found root node "+patchXML.Root.Name.ToString()+"!");
                if (patchXML.Root.Name.ToString() == "Patch")  // Loads up the root node
                {
                    foreach (XElement patchItem in patchXML.Descendants("PatchItem"))
                    {
                        if (patchItem == null)
                        {
                            logging.Error("PatchItem is null!");
                            continue;
                        }
                        logging.Info("Found a <PatchItem> " + patchItem.Name.ToString() + "!");
                        foreach (var patchFile in patchItem.Descendants("PatchFile"))
                        {
                            if (patchFile == null)
                            {
                                logging.Error("PatchFile is null!");
                                continue;
                            }
                            if (patchFile.Attribute("File") == null)
                            {
                                logging.Error("PatchFile has no File attribute!");
                                continue;
                            }
                            logging.Info("Found a <PatchFile> " + patchFile.Name.ToString() + "!");
                            if (File.Exists(Path.Combine("Content\\", patchFile.Attribute("File").Value)))
                            {
                                logging.Info("The file <PatchFile> specifies, " + patchFile.Attribute("File").Value.ToString() + ", exists!");
                                foreach (var patchDelete in patchFile.Descendants("PatchDelete"))
                                {
                                    logging.Info("Found a <PatchDelete> " + patchDelete.Name.ToString() + "!");
                                    //bool done = false;
                                    foreach (var xmlFile in xmls)  // this accesses all files...
                                    {
                                        if(xmlFile.Descendants(patchDelete.Attribute("Name").Value) != null)
                                        {
                                            foreach (var node in xmlFile.Descendants(patchDelete.Attribute("Name").Value))
                                            {
                                                node.Remove();
                                                logging.Info("Deleted node " + node.Name + "!");
                                                //if (patchDelete.Attribute("All").Value == null)
                                                //{
                                                //    logging.Info("Deleting of " + node.Name + " finished because [All] is not set!");
                                                //    done = true;
                                                //    break;
                                                //}
                                                //if (patchDelete.Attribute("All").Value == "false")
                                                //{
                                                //    logging.Info("Deleting of " + node.Name + " finished because [All] is false!");
                                                //    done = true;
                                                //    break;
                                                //}
                                            }
                                            //if (done)
                                            //{
                                            //    break;
                                            //}
                                        }
                                    }
                                }
                            }
                            else
                            {
                                logging.Error("Patch file not found: " + patchFile.Attribute("File").Value);
                            }
                        }
                    }
                }
                else
                {
                    logging.Error("Root node in Patching.xml is invalid! (" + patchXML.Root.Name.ToString() + ")");
                }
            }
            logging.Info("Loading complete!");
        }

        [StarMapUnload]
        public void Cleanup()
        {
            logging.Info("Beggining cleanup...");
            logging.Info("Deleting Content folder...");
            foreach (string file in Directory.GetFiles("Content\\", "*.*", SearchOption.AllDirectories))
            {
                if (file.Contains("KittenPatcher\\"))
                {
                    continue;
                }
                try
                {
                    File.Delete(file);
                }
                catch { }
            }
            logging.Info("Restoring Content from Cache...");
            foreach (string file in Directory.GetFiles("C:\\KittenPatcherCache\\", "*.*", SearchOption.AllDirectories))
            {
                try
                {
                    File.Move(file, "Content\\" + file.Substring(file.IndexOf("C:\\KittenPatcherCache") + "C:\\KittenPatcherCache".Length));
                }
                catch
                {
                    logging.Info("Failed to restore "+file+", skipping...");
                }
            }
            logging.Info("Deleting the no longer needed Cache folder...");
            Directory.Delete("C:\\KittenPatcherCache", true);
            logging.Info("Cleanup complete!");
        }
    }
}
