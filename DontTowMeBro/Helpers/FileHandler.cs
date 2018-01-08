//Date 29 Jan 14
//Author: Michael Stewart
//Notes: Need to implement save system for system state tracking and individual system states using Json and normal file save/load methods
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.IsolatedStorage;
using DontTowMeBro.Notifications;
using Newtonsoft.Json;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Collections.ObjectModel;
using DontTowMeBro.ViewModels;
using Microsoft.Phone.Maps.Controls;


namespace DontTowMeBro.Helpers
{


    public class SystemActiveState
    {
        public bool ParkingMeter { get; set; }
        public bool ParkingZone { get; set; }
        public bool loaded { get; set; }

        public SystemActiveState()
        {
            ParkingMeter = false;
            ParkingZone = false;
            loaded = false;
        }

    }//end class


   
    public struct FindMyCarFile
    {   
        public FindMyCarViewModel file { get; set; }
    }


   [DataContract]
   public struct MeterStateFile
    {
       [DataMember]
       public GpsLoc   loc { get; set; }
       [DataMember]
       public DateTime MeterReturnTime { get; set; }
       [DataMember]
       public DateTime MeterWarningTime { get; set; }
       [DataMember]
       public DateTime MeterStartTime { get; set; }
       [DataMember]
       public bool     active { get; set; }
       [DataMember]
       public int      warnSelectedIndex{ get; set; }
       [DataMember]
       public int      meterSelectedIndex { get; set; }
       [DataMember]
       public bool     firstSaved { get; set; }
       [DataMember]
       public bool     gpsButtonActive { get; set; }
    }//end

   [DataContract]
   public struct ZoneStateFile
   {
       [DataMember]
       public GpsLoc loc { get; set; }
       [DataMember]
       public DateTime parkingMeterReturn { get; set; }
       [DataMember]
       public DateTime parkingMeterStart { get; set; }
       [DataMember]
       public DateTime parkingMeterWarning { get; set; }
       [DataMember]
       public bool active { get; set; }
       [DataMember]
       public bool warnTimeSet { get; set; }
       [DataMember]
       public bool startTimeSet { get; set; }
       [DataMember]
       public bool stopTimeSet { get; set; }
       [DataMember]
       public bool firstSave { get; set; }
       [DataMember]
       public string pzStopText { get; set; }
       [DataMember]
       public string pzStartText { get; set; }
       [DataMember]
       public string pzWarnText { get; set; }
       [DataMember]
       public pzStartTimePacket StartTimePacket { get; set; }
       [DataMember]
       public pzStopTimePacket StopTimePacket { get; set; }
       [DataMember]
       public pzWarningTimePacket WarningTimePacket { get; set; }
       [DataMember]
       public bool gpsButtonActive { get; set; }

   }//end

   [DataContract]
   public struct SettingsFile
   {
       [DataMember]
       public bool gpsPMButtonActive { get; set; }
       [DataMember]
       public bool gpsPZButtonActive { get; set; }
       [DataMember]
       public pzStartTimePacket StartTimePacket { get; set; }
       [DataMember]
       public pzStopTimePacket StopTimePacket { get; set; }
       [DataMember]
       public pzWarningTimePacket WarningTimePacket { get; set; }
       [DataMember]
       public string pzStopText { get; set; }
       [DataMember]
       public string pzStartText { get; set; }
       [DataMember]
       public string pzWarnText { get; set; }
       [DataMember]
       public bool findMyCarActive { get; set; }
       [DataMember]
       public string meterTime { get; set; }
       [DataMember]
       public string warnigTime { get; set; }
       [DataMember]
       public bool userToggle { get; set; }
   }//end

   public struct FileStruct
   {
       public MeterStateFile meterFile;
       public ZoneStateFile zoneFile;
   }//end

 

    public enum StateType { PARKINGMETER, PARKINGZONE, SETTINGS, FINDMYCAR};

    /// <summary>
    /// Main FileHandler
    /// </summary>
    public class FileHandler
    {


        public FileHandler() { }

        //LoadState loadState;
        //SaveState saveState;

        /// <summary>
        /// returns all directories within the isolatedStorage
        /// </summary>
        /// <param name="pattern">File Pattern ex "*"</param>
        /// <param name="storeFile">isolatedStorageFile</param>
        /// <returns>List of directories</returns>
        public List<String> GetAllDirectories(string pattern, IsolatedStorageFile storeFile)
        {

            //get root of the search string
            string root = Path.GetDirectoryName(pattern);

            if (root != "")
            {
                root += "/";
            }

            //retrieve directories
            List<String> directoryLists = new List<string>(storeFile.GetDirectoryNames(pattern));

            //retrieve subdirectories of matches
            for (int i = 0, max = directoryLists.Count; i < max; i++)
            {
                string directory = directoryLists[i] + "/";
                List<String> more = GetAllDirectories(root + directory + "*", storeFile);

                //For each subdirectory found add in base path
                for (int j = 0; j < more.Count; j++)
                {
                    more[j] = directory + more[j];
                }

                //insert subdirectories into the list and update
                //the counter and upper bound
                directoryLists.InsertRange(i + 1, more);
                i += more.Count;
                max += more.Count;

            }//end for
            return directoryLists;
        }//end


        /// <summary>
        /// Returns a list of files in isolatedStore directories
        /// </summary>
        /// <param name="pattern">ex "*"</param>
        /// <param name="storeFile">IsolatedStorageFile</param>
        /// <returns>File List</returns>
        public List<String> GetAllFiles(string pattern, IsolatedStorageFile storeFile)
        {
            //Get root and file portions of the search string
            string fileString = Path.GetFileName(pattern);
            List<String> fileList = new List<string>(storeFile.GetFileNames(pattern));

            //loop through the subdirectories, collect matches,
            //and make seperators consistent

            try
            {
                foreach (string directory in GetAllDirectories("UserSaves", storeFile))
                {
                    foreach (string file in storeFile.GetFileNames(directory + "/" + fileString))
                    {
                        fileList.Add(file);
                    }//end for 2
                }//end for 1
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK);
            }
            return fileList;
        }//end


        public int GetNumberFilesInDirectory(string pattern, IsolatedStorageFile storeFile)
        {

            //Get root and file portions of the search string
            string fileString = Path.GetFileName(pattern);
            int count = 0;

            try
            {
                foreach (string directory in GetAllDirectories("UserSaves", storeFile))
                {
                   storeFile.GetFileNames(directory + "/" + fileString).GetLength(count);
                }//end for 1
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK);
            }

            return count;
        }

        /// <summary>
        /// Loads Last State on start if it exists
        /// </summary>
        /// <returns> true if successful</returns>
        public bool LoadSettings(out object x)
        {
            SettingsFile settingsFile = new SettingsFile();



            string settingsFileJson = null;

            try
            {

                IsolatedStorageFile isolatedStorageFile = null;
                try
                {
                    isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication();

                    if (!isolatedStorageFile.DirectoryExists("CarLocatorSystems"))
                    {
                        isolatedStorageFile.CreateDirectory("CarLocatorSystems");
                    }//end if

                    IsolatedStorageFileStream fileStreamA = null;

                    try
                    {
                        fileStreamA = isolatedStorageFile.OpenFile(@"CarLocatorSystems/Settings.dat",
                        FileMode.OpenOrCreate);

                        using (StreamReader streamReader =
                            new StreamReader(fileStreamA))
                        {
                            fileStreamA = null;


                            settingsFileJson = streamReader.ReadToEnd();

                            if (settingsFileJson.Length == 0)
                            {
                                x = settingsFile;

                                return false;
                            }

                        }
                    }//end using 2
                    finally
                    {
                        if (fileStreamA != null)
                            fileStreamA.Dispose();
                    }
                }//end using 1
                finally
                {
                    if (isolatedStorageFile != null)
                    {
                        isolatedStorageFile.Dispose();
                    }
                }

                x = JsonConvert.DeserializeObject<SettingsFile>(settingsFileJson);
               
                return true; //<= Loaded with Info

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK);
            }

            x = settingsFile;
            return false;
                        
        }

        /// <summary>
        /// Loads Last State on start if it exists
        /// </summary>
        /// <returns> true if successful</returns>
        public bool LoadPreviousPMState(out object x)
        {
            MeterStateFile meterStateFile = new MeterStateFile();

           

            string meterStateFileJson = null;

            try
            {
                using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
                {

                    if (!isolatedStorageFile.DirectoryExists("CarLocatorSystems"))
                    {
                        isolatedStorageFile.CreateDirectory("CarLocatorSystems");
                    }//end if

                    IsolatedStorageFileStream fileStream = null;

                    try
                    {
                        fileStream = isolatedStorageFile.OpenFile(@"CarLocatorSystems/MeterState.dat",
                            FileMode.OpenOrCreate);

                        using (StreamReader streamReader =
                            new StreamReader(fileStream))
                        {
                            fileStream = null;

                            meterStateFileJson = streamReader.ReadToEnd();

                            if (meterStateFileJson.Length == 0)
                            {
                                x = meterStateFile;
                                return false;
                            }
                        }
                    }//end using 2
                    finally
                    {
                        if (fileStream != null)
                            fileStream.Dispose();
                    }

                }//end using 1

                x = JsonConvert.DeserializeObject<MeterStateFile>(meterStateFileJson);

                return true; //<= Loaded with Info

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK);
            }

            x = meterStateFile;
            return false;
            
        }

        /// <summary>
        /// Save Settings Data
        /// </summary>
        /// <returns> true of successful</returns>
        public bool SaveSettings(object x)
        {
            SettingsFile settingsFile = (SettingsFile)x;

            try
            {

                string settingsFileJson = JsonConvert.SerializeObject(settingsFile, Formatting.Indented);

                using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!isolatedStorageFile.DirectoryExists("CarLocatorSystems"))
                    {
                        isolatedStorageFile.CreateDirectory("CarLocatorSystems");
                    }//end if

                    IsolatedStorageFileStream fileStream = null;

                    try
                    {
                        fileStream = isolatedStorageFile.OpenFile(@"CarLocatorSystems/Settings.dat"
                           , FileMode.Create);

                        using (StreamWriter streamWriter = new StreamWriter(fileStream))
                        {
                            fileStream = null;
                            streamWriter.Write(settingsFileJson);

                        }//end using 3
                    }//end using 2
                    finally
                    {
                        if (fileStream != null)
                            fileStream.Dispose();
                    }

                }//end using 1
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK);
            }

            return false;
        }


        /// <summary>
        /// Save Current State on Exit
        /// </summary>
        /// <returns> true of successful</returns>
        public bool SaveCurrentPMState(object x)
        {
            MeterStateFile meterStateFile = (MeterStateFile)x;
           
            try
            {

                string meterStateFileJson = JsonConvert.SerializeObject(meterStateFile, Formatting.Indented);

                using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!isolatedStorageFile.DirectoryExists("CarLocatorSystems"))
                    {
                        isolatedStorageFile.CreateDirectory("CarLocatorSystems");
                    }//end if

                    IsolatedStorageFileStream fileStream = null;

                    try
                    {
                        fileStream = isolatedStorageFile.OpenFile(@"CarLocatorSystems/MeterState.dat"
                           , FileMode.Create);

                        using (StreamWriter streamWriter = new StreamWriter(fileStream))
                        {
                            fileStream = null;
                            streamWriter.Write(meterStateFileJson);
                        }//end using 3
                    }//end using 2
                    finally
                    {
                        if (fileStream != null)
                            fileStream.Dispose();
                    }
                }//end using 1
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK);
            }

            return false;
        }


        /// <summary>
        /// Save Current State on Exit
        /// </summary>
        /// <returns> true of successful</returns>
        public bool SaveCurrentPZState(object x)
        {
            ZoneStateFile zoneStateFile = (ZoneStateFile)x;

            try
            {

                string zoneStateFileJson = JsonConvert.SerializeObject(zoneStateFile, Formatting.Indented);

                using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!isolatedStorageFile.DirectoryExists("CarLocatorSystems"))
                    {
                        isolatedStorageFile.CreateDirectory("CarLocatorSystems");
                    }//end if

                    IsolatedStorageFileStream fileStream = null;

                    try
                    {
                        fileStream = isolatedStorageFile.OpenFile(@"CarLocatorSystems/ZoneState.dat"
                          , FileMode.Create);

                        using (StreamWriter streamWriter = new StreamWriter(fileStream))
                        {
                            fileStream = null;
                            streamWriter.Write(zoneStateFileJson);
                        }//end using 3
                    }//end using 2
                    finally
                    {
                        if (fileStream != null)
                            fileStream.Dispose();
                    }
                }//end using 1
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK);
            }

            return false;
        }


        //////////////////////////////////////
        /// <summary>
        /// Save Current State on Exit
        /// </summary>
        /// <returns> true of successful</returns>
        public bool SaveCurrentFMCState(object x)
        {
            
            FindMyCarFile findMyCarFile = (FindMyCarFile)x;

            try
            {

                string findMyCarFileJson = JsonConvert.SerializeObject(findMyCarFile, Formatting.Indented);

                using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!isolatedStorageFile.DirectoryExists("CarLocatorSystems"))
                    {
                        isolatedStorageFile.CreateDirectory("CarLocatorSystems");
                    }//end if

                    IsolatedStorageFileStream fileStream = null;

                    try
                    {
                        fileStream = isolatedStorageFile.OpenFile(@"CarLocatorSystems/FindMyCarFile.dat"
                          , FileMode.Create);

                        using (StreamWriter streamWriter = new StreamWriter(fileStream))
                        {
                            fileStream = null;
                            streamWriter.Write(findMyCarFileJson);
                        }//end using 3
                    }//end using 2
                    finally
                    {
                        if (fileStream != null)
                            fileStream.Dispose();
                    }
                }//end using 1
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK);
            }

            return false;
        }

        /// <summary>
        /// Loads Last State on start if it exists
        /// </summary>
        /// <returns> true if successful</returns>
        public bool LoadPreviousFMCState(out object x)
        {
            
            FindMyCarFile findMyCarFile = new FindMyCarFile();



            string findMyCarFileJson = null;

            try
            {
                using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
                {

                    if (!isolatedStorageFile.DirectoryExists("CarLocatorSystems"))
                    {
                        isolatedStorageFile.CreateDirectory("CarLocatorSystems");
                    }//end if

                    IsolatedStorageFileStream fileStream = null;

                    try
                    {
                        fileStream = isolatedStorageFile.OpenFile(@"CarLocatorSystems/FindMyCarFile.dat",
                            FileMode.OpenOrCreate);

                        using (StreamReader streamReader =
                            new StreamReader(fileStream))
                        {
                            fileStream = null;
                            findMyCarFileJson = streamReader.ReadToEnd();

                            if (findMyCarFileJson.Length == 0)
                            {
                                x = findMyCarFile;
                                return false;
                            }
                        }
                    }//end using 2
                    finally
                    {
                        if (fileStream != null)
                        {
                            fileStream.Dispose();
                        }
                    }
                }//end using 1

                x = JsonConvert.DeserializeObject<FindMyCarFile>(findMyCarFileJson);

                return true; //<= Loaded with Info

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK);
            }

            x = findMyCarFile;
            return false;



        }

        /// <summary>
        /// Loads Last State on start if it exists
        /// </summary>
        /// <returns> true if successful</returns>
        public bool LoadPreviousPZState(out object x)
        {
            ZoneStateFile zoneStateFile = new ZoneStateFile();



            string zoneStateFileJson = null;

            try
            {
                using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
                {

                    if (!isolatedStorageFile.DirectoryExists("CarLocatorSystems"))
                    {
                        isolatedStorageFile.CreateDirectory("CarLocatorSystems");
                    }//end if

                    IsolatedStorageFileStream fileStream = null;

                    try
                    {
                        fileStream = isolatedStorageFile.OpenFile(@"CarLocatorSystems/ZoneState.dat",
                            FileMode.OpenOrCreate);

                        using (StreamReader streamReader =
                            new StreamReader(fileStream))
                        {
                            fileStream = null;
                            zoneStateFileJson = streamReader.ReadToEnd();

                            if (zoneStateFileJson.Length == 0)
                            {
                                x = zoneStateFile;
                                return false;
                            }
                        }
                    }//end using 2
                    finally
                    {
                        if (fileStream != null)
                        {
                            fileStream.Dispose();
                        }
                    }
                }//end using 1

                x = JsonConvert.DeserializeObject<ZoneStateFile>(zoneStateFileJson);

                return true; //<= Loaded with Info

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK);
            }

            x = zoneStateFile;
            return false;



        }

        /// <summary>
        /// Save User File
        /// </summary>
        /// <param name="x">SaveFileStruct</param>
        /// <returns> true of successful</returns>
        public bool SaveUserFile(object x, string FileName)
        {
            FileStruct saveFileStruct = (FileStruct)x;

            try
            {

                string meterStateFileJson = JsonConvert.SerializeObject(saveFileStruct, Formatting.Indented);

                using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!isolatedStorageFile.DirectoryExists("UserSaves"))
                    {
                        isolatedStorageFile.CreateDirectory("UserSaves");
                    }//end if

                    IsolatedStorageFileStream fileStream = null;

                    try
                    {
                        fileStream = isolatedStorageFile.OpenFile(@"UserSaves/" + FileName + ".dat"
                           , FileMode.Create);

                        using (StreamWriter streamWriter = new StreamWriter(fileStream))
                        {
                            fileStream = null;
                            streamWriter.Write(meterStateFileJson);
                        }//end using 3
                    }//end using 2
                    finally
                    {
                        if (fileStream != null)
                            fileStream.Dispose();
                    }
                }//end using 1
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK);
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public bool LoadUserFile(out object x, string FileName)
        {
            FileStruct loadFileStruct = new FileStruct();
            string loadedFile = null;

            try
            {
                using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
                {

                    if (!isolatedStorageFile.DirectoryExists("UserSaves"))
                    {
                        isolatedStorageFile.CreateDirectory("UserSaves");
                    }//end if

                    IsolatedStorageFileStream fileStream = null;

                    try
                    {
                        fileStream = isolatedStorageFile.OpenFile(@"UserSaves/" + FileName,
                            FileMode.OpenOrCreate);

                        using (StreamReader streamReader =
                            new StreamReader(fileStream))
                        {
                            fileStream = null;
                            loadedFile = streamReader.ReadToEnd();

                            if (loadedFile.Length == 0)
                            {
                                x = loadFileStruct;
                                return false;
                            }
                        }
                    }//end using 2
                    finally
                    {
                        if (fileStream != null)
                            fileStream.Dispose();
                    }
                }//end using 1

                x = JsonConvert.DeserializeObject<FileStruct>(loadedFile);

                return true; //<= Loaded with Info
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK);
            }

            x = loadFileStruct;
            return false;
        }


        /// <summary>
        /// Saves the current state of active systems
        /// ex.
        /// ParkingMeterNotifications.Active = true
        /// </summary>
        /// <param name="x">SystemActiveState object</param>
        /// <returns></returns>
        public bool SaveActiveSystemStates(SystemActiveState x)
        {
            try
            {
                string activeSystemStateJson = JsonConvert.SerializeObject(x, Formatting.Indented);

                using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!isolatedStorageFile.DirectoryExists("CarLocatorSystems"))
                    {
                        isolatedStorageFile.CreateDirectory("CarLocatorSystems");
                    }//end if

                    IsolatedStorageFileStream fileStream = null;

                    try
                    {
                    fileStream = isolatedStorageFile.OpenFile(@"CarLocatorSystems/ActiveSystemState.dat"
                        , FileMode.Create);
                    
                        using (StreamWriter streamWriter = new StreamWriter(fileStream))
                        {
                            fileStream = null;
                            streamWriter.Write(activeSystemStateJson);
                        }//end using 3
                    }//end using 2
                    finally
                    {
                        if (fileStream != null)
                            fileStream.Dispose();
                    }
                }//end using 1
                return true;
            }//end try
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error FileHandler.cs / SaveActiveSystemStates()", MessageBoxButton.OK);
            }
            
            return false;
        }

        /// <summary>
        /// Loads the active system state object
        /// </summary>
        /// <param name="x">Active system state object</param>
        /// <returns>true is successful</returns>
        public SystemActiveState LoadActiveSystemStates()
        {
            SystemActiveState x = new SystemActiveState();
            string activeStateFile = null;

            try
            {
                using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
                {

                    if (!isolatedStorageFile.DirectoryExists("CarLocatorSystems"))
                    {
                        isolatedStorageFile.CreateDirectory("CarLocatorSystems");
                    }//end if

                    IsolatedStorageFileStream fileStream = null;

                    try
                    {
                        fileStream = isolatedStorageFile.OpenFile(@"CarLocatorSystems/ActiveSystemState.dat",
                            FileMode.OpenOrCreate);

                        using (StreamReader streamReader =
                            new StreamReader(fileStream))
                        {
                            fileStream = null;
                            activeStateFile = streamReader.ReadToEnd();

                            if (activeStateFile.Length == 0)
                                return x;
                        }
                    }//end using 2
                    finally
                    {
                        if (fileStream != null)
                        {
                            fileStream.Dispose();
                        }
                    }
                }//end using 1

                x = JsonConvert.DeserializeObject<SystemActiveState>(activeStateFile);

                x.loaded = true;

                return x; //<= Loaded with Info

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK);
            }
            
            return x;
        }

    }

    /// <summary>
    /// Load Prev State
    /// </summary>
    public class LoadState
    {
        
        FileHandler newHandler = new FileHandler();
        /// <summary>
        /// Load State
        /// </summary>
        /// <param name="stateType">PARKINGMETER</param>
        public object LoadNewState(StateType stateType)
        {
            object x = null;

            switch (stateType)
            {
                case StateType.PARKINGMETER:
                    {
                        newHandler.LoadPreviousPMState(out x);
                        
                    }break;
                case StateType.PARKINGZONE:
                    {
                        newHandler.LoadPreviousPZState(out x);
                    }break;
                case StateType.SETTINGS:
                    {
                        newHandler.LoadSettings(out x);
                    }break;
                case StateType.FINDMYCAR:
                    {
                        newHandler.LoadPreviousFMCState(out x);
                    }break;
            }//end switch

            return x;
        }

        /// <summary>
        /// Loads ActiveSystemState Object
        /// </summary>
        /// <param name="x">SystemStateObject</param>
        public void LoadActiveSystemStates(out SystemActiveState x)
        {
           x = newHandler.LoadActiveSystemStates();
        }

    }

    /// <summary>
    /// Save State
    /// </summary>
    public class SaveState
    {
        FileHandler newHandler = new FileHandler();

        /// <summary>
        /// Save State
        /// </summary>
        /// <param name="stateType">PARKINGMETER</param>
        /// <param name="x">StateFile Type</param>
        ///<remarks>
        ///Remove case statements and save all systems
        /// </remarks>
        public void  SaveSystemState(StateType stateType, object x)
        {
            switch (stateType)
            {
                case StateType.PARKINGMETER:
                    {
                        newHandler.SaveCurrentPMState(x);
                    } break;
                case StateType.PARKINGZONE:
                    {
                        newHandler.SaveCurrentPZState(x);
                    }break;
                case StateType.SETTINGS:
                    {
                        newHandler.SaveSettings(x);
                    }break;
                case StateType.FINDMYCAR:
                    {
                        newHandler.SaveCurrentFMCState(x);
                    }break;
            }//end switch
        }

        /// <summary>
        /// Saves the active system state
        /// </summary>
        /// <param name="x">StateFile Type</param>
        public void SaveActiveSystemStates(SystemActiveState x)
        {
            newHandler.SaveActiveSystemStates(x);
        }
    }


    /// <summary>
    /// Saves user file to device
    /// </summary>
    public class UserSaveFile
    {

        FileHandler fh = new FileHandler();
        
        /// <summary>
        /// Save all Systems for User
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public bool SaveUserFile(object x,string FileName)
        {

            fh.SaveUserFile(x, FileName);

            return false;
        }
    }

    /// <summary>
    /// Loads users file selection
    /// </summary>
    public class LoadUserFile
    {
        FileHandler fh = new FileHandler();

        public void LoadUserFiles(out object x, string FileName)
        {
            fh.LoadUserFile(out x, FileName);
        }
    }

    #region LoadFileNames



    /// <summary>
    /// Creates a userfile structure that stores filename and ID, can be observed 
    /// </summary>
    public class UserLoadedFileName : INotifyPropertyChanged 
    {
        private string _file;
        private int _ID;

        public string file
        {

            get { return _file; }

            set
            {
                if (_file != value)
                {
                    _file = value;
                    OnPropertyChange("file");
                }
            }
        }//end file

        public int ID
        {
            get { return _ID; }
            set
            {
                if (_ID != value)
                {
                    _ID = value;
                    OnPropertyChange("ID");
                }
            }

        }//end ID

        public UserLoadedFileName() { }


        public UserLoadedFileName(string fileName, int id)
        {
            file = fileName;
            ID = id;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sPropertyname"></param>
        protected void OnPropertyChange([CallerMemberName] string sPropertyName = null)
        {
            PropertyChangedEventHandler tempEvent = PropertyChanged;
            if (tempEvent != null)
            {
                tempEvent(this, new PropertyChangedEventArgs(sPropertyName));
            }

        }//end OnPropertyChange
    }



    /// <summary>
    /// Access IsolatedStore Directories/File Lists
    /// </summary>
    public class AccessIsoStore : INotifyPropertyChanged
    {
        FileHandler fh = new FileHandler();

        public AccessIsoStore()
        {
            this.Files = new ObservableCollection<UserLoadedFileName>();
        }

        public ObservableCollection<UserLoadedFileName> Files { get; private set; }

        IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();

        /// <summary>
        /// Return List of files in IsoStore
        /// </summary>
        /// <param name="filePath">File Pathname. use "*.dat" for all files</param>
        /// <returns>list of files</returns>
        public ObservableCollection<UserLoadedFileName> returnFiles(string filePath)
        {

            //ObservableCollection<UserFile> fileList = new ObservableCollection<UserFile>();
            //UserLoadedFile userLoadedFile = null;
            int id = 0;
            foreach (string file in fh.GetAllFiles(filePath, isoStore))
            {
                //userLoadedFile = new UserLoadedFile(file);

                this.Files.Add(new UserLoadedFileName(file, id));
                id++;
            }

            return this.Files;
        }


        public bool areFilesLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns files found in isolatedStorage 
        /// </summary>
        /// <param name="filePath"></param>
        public void returnFiles()
        {
            this.Files.Clear();

            int oldCount = fh.GetNumberFilesInDirectory("*.dat", isoStore);
            int newCount = 0;
            //ObservableCollection<UserFile> fileList = new ObservableCollection<UserFile>();
            //UserLoadedFile userLoadedFile = null;
            int id = 0;
            foreach (string file in fh.GetAllFiles("*.dat", isoStore))
            {
                //userLoadedFile = new UserLoadedFile(file);
                this.Files.Add(new UserLoadedFileName(file, id));
                id++;
            }

            newCount = fh.GetNumberFilesInDirectory("*.dat", isoStore);

            if (oldCount == newCount)
                areFilesLoaded = true;
            else
                areFilesLoaded = false;


        } 
    


        /// <summary>
        /// return list of directories
        /// </summary>
        /// <param name="filePath">Directory Pathname use "*" for all directories</param>
        /// <returns>list of directories onIsoStore</returns>
        public List<string> ReturnDirectories(string filePath)
        {
            List<string> dirList = null;

            foreach (string directory in fh.GetAllDirectories(filePath, isoStore))
            {
                dirList.Add(directory);
            }

            return dirList;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Updates if sProperyName is updated
        /// </summary>
        /// <param name="sPropertyname">Property to watch for update</param>
        protected virtual void OnPropertyChange(
            [CallerMemberName] string sPropertyName = null)
        {
            PropertyChangedEventHandler tempEvent = PropertyChanged;
            if (tempEvent != null)
            {
                tempEvent(this, new PropertyChangedEventArgs(sPropertyName));
            }

        }//end OnPropertyChange
    }//end AccessIsoStore
    #endregion
}
