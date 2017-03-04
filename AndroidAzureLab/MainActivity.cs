using Android.App;
using Android.Widget;
using Android.OS;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System.Net;
using System.IO;
using XamarinDiplomado.Participants.Startup;

namespace AndroidAzureLab
{
    [Activity(Label = "AndroidAzureLab", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        ImageView ImagenDrop;
        string archivoLocal;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);

            Startup startup = new Startup("Aland Ruiz Castro","ar1987@hotmail.com",1,1);
            startup.Init();

            Button btnImagen = FindViewById<Button>(Resource.Id.btnRealizar);
            ImagenDrop = FindViewById<ImageView>
                (Resource.Id.imagen);
            btnImagen.Click += ArchivoImagen;

        }

        async void ArchivoImagen(object sender, EventArgs e)
        {
            try
            {
                var ruta = await DescargaImagen();
                Android.Net.Uri rutaImagen = Android.Net.Uri.Parse(ruta);
                ImagenDrop.SetImageURI(rutaImagen);

                CloudStorageAccount cuentaAlmacenamiento =
                    CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=subirimagen;AccountKey=hB623e/swtKMq/3EA++gCp4UAmGnyHBVJYidnrgo+y5sz1IBb4wExZwYT5JmrYNAq3HdQmBiRj10bm/29dIY0A==");
                CloudBlobClient clienteBlob = cuentaAlmacenamiento.CreateCloudBlobClient();
                CloudBlobContainer contenedor = clienteBlob.GetContainerReference("laboratorio1");
                CloudBlockBlob recursoblob = contenedor.GetBlockBlobReference(archivoLocal);
                await recursoblob.UploadFromFileAsync(ruta);

                Toast.MakeText(this, "Guardado en Azure Storage Blob", ToastLength.Long).Show();

                CloudTableClient tableClient = cuentaAlmacenamiento.CreateCloudTableClient();

                CloudTable table = tableClient.GetTableReference("Ubicaciones");

                await table.CreateIfNotExistsAsync();

                UbicacionEntity ubica = new UbicacionEntity(archivoLocal, "Colombia");
                ubica.Latitud = 11.003040;
                ubica.Localidad = "Barranquilla";
                ubica.Longitud = -74.796299;

                TableOperation insertar = TableOperation.Insert(ubica);
                await table.ExecuteAsync(insertar);

                Toast.MakeText(this, "Guardado en Azure Storage Table NoSQL", ToastLength.Long).Show();
            }
            catch (Exception exc)
            {
                Toast.MakeText(this, exc.Message, ToastLength.Long).Show();
            }
        }

        public async Task<string> DescargaImagen()
        {

            WebClient client = new WebClient();
            byte[] imageData = await client.DownloadDataTaskAsync("https://upload.wikimedia.org/wikipedia/commons/thumb/b/bb/Panor%C3%A1mica_general_de_Barranquilla.JPG/260px-Panor%C3%A1mica_general_de_Barranquilla.JPG");

            string documentspath = System.Environment.GetFolderPath
                (System.Environment.SpecialFolder.Personal);
            archivoLocal = "mifoto1.jpg";
            string localpath = Path.Combine(documentspath, archivoLocal);
            File.WriteAllBytes(localpath, imageData);
            return localpath;
        }
    }
    public class UbicacionEntity : TableEntity
    {
        public UbicacionEntity(string Archivo, string Pais)
        {
            this.PartitionKey = Archivo;
            this.RowKey = Pais;
        }
        public UbicacionEntity() { }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public string Localidad { get; set; }
    }


}

