using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;

namespace OktoberQuest
{
    public class SaveLoad
    {
        StorageDevice device;
        string containerName = "Saves";
        string filename = "OktoberQuest.sav";
        public SaveGame SaveData;

        [Serializable]
        public struct SaveGame
        {
            public string profileName;
        }

        public void InitiateSave(int playerNumber, string pName)  
        {
            SaveData = new SaveGame()
            {
                    profileName = pName,
            };
            PlayerIndex player = mapIntToIndex(playerNumber);
            SelectSaveDevice(player);
        }

        void SelectSaveDevice(PlayerIndex player)
        {
            device = null;
            StorageDevice.BeginShowSelector(player, this.SaveToDevice, null); 
        }

        void SaveToDevice(IAsyncResult result)
        {
            device = StorageDevice.EndShowSelector(result);;
            if (device != null && device.IsConnected)
            {
                IAsyncResult r = device.BeginOpenContainer(containerName, null, null);
                result.AsyncWaitHandle.WaitOne();
                StorageContainer container = device.EndOpenContainer(r);
                if (container.FileExists(filename))
                {
                    container.DeleteFile(filename);
                }
                Stream stream = container.CreateFile(filename);
                XmlSerializer serializer = new XmlSerializer(typeof(SaveGame));
                serializer.Serialize(stream, SaveData);
                stream.Close();
                container.Dispose();
                result.AsyncWaitHandle.Close();
            }
        }

        public void InitiateLoad(int playerNumber)  
        {
            PlayerIndex player = mapIntToIndex(playerNumber);
            device = null;
            StorageDevice.BeginShowSelector(player, this.LoadFromDevice, null);
        }

        void LoadFromDevice(IAsyncResult result)
        {
            device = StorageDevice.EndShowSelector(result);
            IAsyncResult r = device.BeginOpenContainer(containerName, null, null);
            result.AsyncWaitHandle.WaitOne();
            StorageContainer container = device.EndOpenContainer(r);
            result.AsyncWaitHandle.Close();
            if (container.FileExists(filename))
            {
                Stream stream = container.OpenFile(filename, FileMode.Open);
                XmlSerializer serializer = new XmlSerializer(typeof(SaveGame));
                SaveData = (SaveGame)serializer.Deserialize(stream);
                stream.Close();
                container.Dispose();
                //Update the game based on the save game file
            }
            else
            {
                SaveData.profileName = null;
            }
        }
        /// <summary>
        /// This method deletes a file previously created .
        /// </summary>
        /// <param name="device"></param>
        void DoDelete(IAsyncResult result)
        {
            device = StorageDevice.EndShowSelector(result);
            IAsyncResult r =
                device.BeginOpenContainer(containerName, null, null);

            // Wait for the WaitHandle to become signaled.
            r.AsyncWaitHandle.WaitOne();

            StorageContainer container = device.EndOpenContainer(r);

            // Close the wait handle.
            r.AsyncWaitHandle.Close();
                        
            if (container.FileExists(filename))
            {
                container.DeleteFile(filename);
            }

            // Dispose the container, to commit the change.
            container.Dispose();
        }

        PlayerIndex mapIntToIndex(int profile)
        {
            PlayerIndex player = PlayerIndex.One;
            switch (profile)
            {
                case 1:
                    player = (PlayerIndex.One);
                    break;
                case 2:
                    player = (PlayerIndex.Two);
                    break;
                case 3:
                    player = (PlayerIndex.Three);
                    break;
                case 4:
                    player = (PlayerIndex.Four);
                    break;
            }
            return player;
        }
    }
}
