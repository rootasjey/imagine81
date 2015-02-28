﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using System.Linq;
using System.IO;

namespace Imagine.Common
{
    public class MyDataSerializer<TheDataType>
    {
        public static async Task SaveObjectsAsync(TheDataType sourceData, String targetFileName)
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(targetFileName, CreationCollisionOption.ReplaceExisting);

            var outStream = await file.OpenStreamForWriteAsync(); // ERREUR NON GEREE ICI

            DataContractSerializer serializer = new DataContractSerializer(typeof(TheDataType));
            serializer.WriteObject(outStream, sourceData);
            await outStream.FlushAsync();
            //outStream.Close();
            outStream.Dispose();
        }

        public static async Task<TheDataType> RestoreObjectsAsync(string filename)
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(filename);

            var inStream = await file.OpenStreamForReadAsync();

            //Deserialize the objetcs
            DataContractSerializer serializer = new DataContractSerializer(typeof(TheDataType));
            TheDataType data = (TheDataType)serializer.ReadObject(inStream);
            //inStream.Close();
            inStream.Dispose();

            return data;
        }
    }
}
