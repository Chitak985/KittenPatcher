using StarMap.API;
using System.Reflection;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        public string dllPath = Path.GetFullPath(Assembly.GetExecutingAssembly().Location);
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
            if (Directory.Exists("ContentKittenPatcherCache"))
            {
                logging.Info("Cache from an old run is found, deleting it...");
                Directory.Delete("ContentKittenPatcherCache", true);
            }
            else
            {
                logging.Info("Creating Cache folder...");
            }
            Directory.CreateDirectory("ContentKittenPatcherCache");

            logging.Info("Creating a command line file for KittenPatcher's Initialization...");
            StreamWriter cmdFileInit = File.CreateText("Content\\KittenPatcher\\KittenPatcherInitialization.cmd");
            cmdFileInit.Write("@echo off\necho ----- KittenPatcher Initializer -----\necho Copying Content folder to ContentKittenPatcherCache...\nxcopy \""+dllPath.Substring(0, dllPath.IndexOf("\\KittenPatcher"))+"\" \""+dllPath.Substring(0, dllPath.IndexOf("\\KittenPatcher"))+"KittenPatcherCache\" /I /E /-Y\necho Content folder move complete. Finishing in 5 seconds...\ntimeout /t 5 /nobreak > NUL");
            cmdFileInit.Close();

            logging.Info("Running the file...");
            System.Diagnostics.Process initFile = System.Diagnostics.Process.Start("Content\\KittenPatcher\\KittenPatcherInitialization.cmd");
            initFile.WaitForExit();

            logging.Info("Cache move finished!");

            // ----- PATCHING -----

            // Load all XML files
            logging.Info("Loading XML files...");
            List<XDocument> xmls = new List<XDocument>();
            foreach (string xmlFile in Directory.GetFiles("Content\\", "*.xml", SearchOption.AllDirectories))
            {
                xmls.Add(XDocument.Load(xmlFile));
            }
            
            foreach (string patchPath in Directory.GetFiles("Content\\", "Patching.xml", SearchOption.AllDirectories))                                  // Load all Patching.xml files to load patches from
            {
                logging.Info("A Patching.xml file was found at "+patchPath+"!");
                XDocument patchXML = XDocument.Load(patchPath);                                                                                         // Load file as an XML document
                if (patchXML == null)
                {
                    logging.Error("Something is VERY wrong!");                                                                                          // Realisticlly shouldn't happen
                    continue;
                }
                if (patchXML.Root == null)
                {
                    logging.Error("Patching.xml has no root node!");                                                                                    // Idk how would happen but whatever
                    continue;
                }
                logging.Info("Found root node "+patchXML.Root.Name.ToString()+"!");
                if (patchXML.Root.Name.ToString() == "Patch")                                                                                           // Loads up the root node
                {
                    foreach (XElement patchItem in patchXML.Descendants("PatchItem"))                                                                   // Iterate through all <PatchItem>s
                    {
                        if (patchItem == null)                                                                                                          // Make sure patchItem isn't null
                        {
                            logging.Error("PatchItem is null!");
                            continue;
                        }
                        logging.Info("Found a <PatchItem> " + patchItem.Name.ToString() + "!");                                                         // Confirm that a <PatchItem> was found
                        foreach (var patchFile in patchItem.Descendants("PatchFile"))                                                                   // Iterate through all <PatchFile>s
                        {
                            if (patchFile == null)                                                                                                      // Make sure patchFile isn't null
                            {
                                logging.Error("PatchFile is null!");
                                continue;
                            }
                            if (patchFile.Attribute("File") == null)                                                                                    // Check if <PatchFile> has a file attribute
                            {
                                logging.Error("PatchFile has no File attribute!");
                                continue;
                            }
                            XAttribute fileToPatch = patchFile.Attribute("File");                                                                       // Set the file to be patched to a separate variable to avoid null warnings
                            logging.Info("Found a <PatchFile> " + patchFile.Name.ToString() + "!");                                                     // Confirm that the <PatchFile> was found
                            if (File.Exists(Path.Combine("Content\\", fileToPatch.Value)))                                                              // Check if the <PatchFile> file exists
                            {
                                logging.Info("The file <PatchFile> specifies, " + fileToPatch.Value + ", exists!");                                     // Confirm that the file specified exists
                                foreach (var patchDelete in patchFile.Descendants("PatchDelete"))                                                       // Iterate through all <PatchDelete>
                                {
                                    logging.Info("Found a <PatchDelete> " + patchDelete.Name.ToString() + "!");                                         // Confirm that a <PatchDelete> was found
                                    XDocument xmlFile = XDocument.Load(Path.Combine("Content\\", fileToPatch.Value));                                   // Load specified .xml file
                                    if (xmlFile.Descendants(patchDelete.Attribute("Name").Value) != null)                                               // Make sure Descendants isn't null and can be used
                                    {
                                        List<XElement> nodes = [.. xmlFile.Descendants(patchDelete.Attribute("Name").Value)];                           // Obtain all nodes to remove
                                        foreach (var node in nodes)                                                                                     // Iterate and remove them, this is now independent from Descendants
                                        {
                                            string tmp = "";                                                                                            // Create a temporary string for formatted attributes
                                            foreach (var att in node.Attributes())                                                                      // Iterate through all attributes
                                            {
                                                tmp += att.Name.ToString()+": "+att.Value+", ";                                                         // Add formatted attributes to the temporary string
                                            }
                                            logging.Info("Deleting node "+node.Name+" with attributes {"+tmp.Substring(0,tmp.Length-2)+"}!");           // Log node being deleted with its attributes
                                            try                                                                                                         // Protect from an exception to keep the game running
                                            {
                                                node.Remove();                                                                                          // Remove the element
                                                logging.Info("Node removal successful!");
                                            }
                                            catch (Exception ex)                                                                                        // Catch an exception if one happens
                                            {
                                                logging.Info("Node removal failed with exception "+ex.Message+" with stack trace "+ex.StackTrace+"!");  // Log the error so it doesn't crash the game
                                            }
                                        }
                                    }
                                    xmlFile.Save(Path.Combine("Content\\", fileToPatch.Value));                                                         // Save the .xml back to where it was loaded from
                                    logging.Info("XML was patched and saved successfully!");                                                            // Confirm that saving succeeded.
                                }
                            }
                            else
                            {
                                logging.Error("Patch file not found: " + fileToPatch.Value);                                                            // Log that file wasn't found and skip it
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
            if (Directory.Exists("ContentKittenPatcherCache"))
            {
                logging.Info("Cache is found successfully, continuing cleanup...");
            }
            else
            {
                logging.Error("The cache folder is missing! The Content folder is now corrupted with KittenPatcher patches, if any were executed. Please send this log to the developer, Chitak985, using the KSA Modding discord server (https://discord.gg/6tZwQfCtFu). PLEASE don't send logs in the main help channel, instead use the KittenPatcher forum channel (https://discord.com/channels/1439383096813158702/1444048215170089185).");
                return;
            }

            logging.Info("Removing directories in the Content folder...");
            foreach (var contentFolder in Directory.GetDirectories("Content"))
            {
                if (!contentFolder.Contains("KittenPatcher"))
                {
                    Directory.Delete(contentFolder,true);
                }
            }

            logging.Info("Moving directories from cache to the Content folder...");
            foreach (var contentFolder in Directory.GetDirectories("ContentKittenPatcherCache"))
            {
                if (!contentFolder.Contains("KittenPatcher"))
                {
                    Directory.Move(contentFolder,"Content\\"+contentFolder.Split("\\").Last());
                }
            }

            logging.Info("Removing the no longer needed cache folder...");
            Directory.Delete("ContentKittenPatcherCache", true);

            logging.Info("Cleanup complete!");
        }
    }
}
