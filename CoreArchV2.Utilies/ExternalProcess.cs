namespace CoreArchV2.Utilies
{
    public static class ExternalProcess
    {
        public static string SetPascalCaseNameWithSpace(string name)
        {
            var arr = name.Split(' ').Where(w => w != "").ToArray();

            for (int i = 0; i < arr.Length; i++)
            {
                var temp = arr[i];
                temp = temp.Replace(" ", "").ToLower();
                temp = temp.Substring(0, 1).ToUpper() + temp.Substring(1, temp.Length - 1);
                arr[i] = temp;
            }

            return string.Join(" ", arr);
        }
    }
}
