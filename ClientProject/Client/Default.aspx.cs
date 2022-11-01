using System;
using System.Web.UI;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.IO.Compression;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace Client
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        // Sends JSON to server
        private void SerializeToJsonAndWrite(string objectToWrite)
        {
            string url = "https://localhost:44370";
            JObject jsonObject = new JObject
                {
                    { "FileName", "Name" },
                    { "ContentTree", objectToWrite }
                };

            using (HttpClient client = new HttpClient())
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/api/ZipFile"))
            {
                client.BaseAddress = new Uri(url);
                string base64EncodedAuthenticationString =
                    Convert.ToBase64String(ASCIIEncoding.UTF8.GetBytes($"{oUsername.Value}:{oPassword.Value}"));
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + base64EncodedAuthenticationString);

                StringContent content = new StringContent(JsonConvert.SerializeObject(jsonObject), Encoding.UTF8,
                    "application/json");

                using (HttpResponseMessage response = client.PostAsync("/api/ZipFile", content).Result)
                {
                    string text = response.Content.ReadAsStringAsync().Result;

                    if (!string.IsNullOrEmpty(text) && !response.IsSuccessStatusCode)
                        Response.Write("<script>alert('" + text + "')</script>");
                    else if (!response.IsSuccessStatusCode)
                        Response.Write("<script>alert('Request failed with status code " + response.StatusCode.ToString() + "')</script>");
                    else 
                        Response.Write("<script>alert('Request completed successfully')</script>");
                }
            }
        }

        // tree class to collect folders and files from ZIP in hierachical view
        // weird to have a such code in client side but in accordance with description
        private class TreeNode<T>
        {
            List<TreeNode<T>> Children = new List<TreeNode<T>>();

            public T Item { get; set; }

            public TreeNode(T item)
            {
                Item = item;
            }

            public TreeNode<T> AddChild(T item)
            {
                TreeNode<T> nodeItem = new TreeNode<T>(item);
                Children.Add(nodeItem);
                return nodeItem;
            }

            public bool IsChildExist(T item, out TreeNode<T> foundChild)
            {
                bool result = false;
                foundChild = null;

                foreach (TreeNode<T> child in Children)
                {
                    if (child.Item.Equals(item))
                    {
                        result = true;
                        foundChild = child;
                        break;
                    }
                }
                    
                return result;
            }

            private string Encrypt(string text)
            {
                byte[] src = Encoding.UTF8.GetBytes(text.ToString());
                byte[] key = Encoding.ASCII.GetBytes("0123456789abcdef");
                RijndaelManaged aes = new RijndaelManaged();
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;
                aes.KeySize = 128;

                using (ICryptoTransform encrypt = aes.CreateEncryptor(key, null))
                {
                    byte[] dest = encrypt.TransformFinalBlock(src, 0, src.Length);
                    encrypt.Dispose();
                    return Convert.ToBase64String(dest);
                }
            }

            public string ToString(string root)
            {
                string json; 
                string child_json = string.Empty;

                if (Children.Count > 0)
                {
                    json = "\"" + Encrypt(Item.ToString()) + "\": {0}";
                    foreach (TreeNode<T> child in Children)
                        child_json += child.ToString(string.Empty);
                }
                else
                {
                    json = "\"" + Encrypt(Item.ToString()) + "\": [], ";
                }

                if (child_json.EndsWith(", "))
                    child_json = child_json.Substring(0, child_json.Length - 2);

                return json.Replace("{0}", 
                    !string.IsNullOrEmpty(child_json) ?
                        !string.IsNullOrEmpty(root) ? "{ " + child_json + " }" : "{ " + child_json + " }, " :
                        "");
            }
        }

        // recursive function for refactoring list of ZIP files to tree form
        private TreeNode<string> GetFilesTree(string root, TreeNode<string> parent, ZipArchiveEntry[] entries)
        {
            root = root.Replace("{root}", "");
            ZipArchiveEntry[] children = entries.Where(w => w.FullName.StartsWith(root)).ToArray();

            foreach (ZipArchiveEntry child in children)
            {
                string rootTemplate = string.IsNullOrEmpty(root) ? string.Empty : root + "/";
                string newChildName = child.FullName.Substring(rootTemplate.Length);
                if (newChildName.IndexOf("/") >= 0)
                    newChildName = newChildName.Substring(0, newChildName.IndexOf("/"));

                TreeNode<string> newChild = null;

                if (child.FullName.Substring(rootTemplate.Length).IndexOf("/") >= 0)
                {
                    if (!parent.IsChildExist(newChildName, out newChild))
                    {
                        newChild = parent.AddChild(newChildName);
                        GetFilesTree(rootTemplate + newChild.Item, newChild, entries.Where(w => w.FullName.StartsWith(root)).ToArray());
                    }    
                }
                else
                {
                    if (!parent.IsChildExist(newChildName, out newChild))
                        parent.AddChild(child.Name);
                }
            }

            return parent;
        }

        // event on submit from to send data
        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(oFile.PostedFile.FileName))
            {
                Response.Write("<script>alert('ZIP file is not selected')</script>");
                return;
            }

            TreeNode<string> root = null;
            ZipArchive archive = null;

            try
            {
                archive = new ZipArchive(oFile.PostedFile.InputStream);
                string rootName = "{root}";

                root = new TreeNode<string>(rootName);
                GetFilesTree(rootName, root, archive.Entries.Where(w => !string.IsNullOrEmpty(w.Name)).ToArray());
            }
            catch
            {
                Response.Write("<script>alert('ZIP file has wrong format or is not readable')</script>");
                return;
            }
            finally
            {
                if (archive != null)
                    archive.Dispose();
            }

            if (root != null)
                SerializeToJsonAndWrite("{ " + root.ToString(root.Item) + " }");
        }
    }
}