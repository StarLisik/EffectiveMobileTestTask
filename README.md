# Примечание
Приложение проводит вывод заказов из списка заказов на основе заданного фильтра по району и дате.

Полный список заказов находится в файле *_Orders.txt*. В него можно добавлять или удалять записи. В файле *_config.txt* находятся входные параметры, на основе которых будет происходить фильтрация. Их также можно изменять для различных результатов.
 
## 1. Запуск приложения

### 1.1
 **а)** Для запуска приложения через консоль, необходимо перейти в консоли в папку с исполняемым файлом **(bin/Release/net8.0)** и написать по шаблону ```EffectiveMobileTestTask.exe название_района yyyy-mm-dd hh:mm:ss```.

 **б)** Если не указать параметры через консоль или запустить приложение двойным кликом по исполняемому файлу **(bin/Release/net8.0/EffectiveMobileTestTask.exe)**, то параметры будут взяты автоматически из конфигурационного файла *_config.txt* в той же папке.

## 2. Просмотр результатов и логов

### 2.1
После запуска приложения в консоль будет выведена информация, которая будет продублирована в лог файл *_deliveryLog.txt*. Результаты работы программы будут находится в файле *_deliveryOrders.txt*.


### 3. Unit Test

### 2.1
Для приложения были лобавлены 2 юнит теста для проверки методов ```ValidateParametersTest()``` и ```ValidateOrdersTest()```.

**а)** ```ValidateParametersTest()``` Проверка входных параметров
```C#
[TestMethod()]
public void ValidateParametersTest()
{
    // Arrange
    string[] args = new string[0];  // Недостаточно аргументов
    string configFilePath = "config.txt";
    File.WriteAllText(configFilePath, "Север 2024-11-23 15:33:29");

    // Act
    string[] result = Program.ValidateParameters(args);

    // Assert
    CollectionAssert.AreEqual(new string[] { "Север", "2024-11-23 15:33:29" }, result);

    if (File.Exists(configFilePath))
        File.Delete(configFilePath);

}
```
**б)** ```ValidateOrdersTest()``` Проверка заказов на валидность
```C#
[TestMethod()]
public void ValidateOrdersTest()
{
    // Arrange
    List<string> lines = new List<string>();
    string configFilePath = "Orders.txt";
    File.WriteAllText(configFilePath,
        "1 15.33333 Север 2024-11-23 15:33:32\r\n" +
        "2 4 Юг 2024-10-24 20:10:05\r\n3 8 Запад 2024-10-24 20:10:05\r\n" +
        "4 3.2 Восток 2024-10-24 20:10:05\r\n" +
        "5 11 Центр 2024-10-24 20:10:05\r\n" +
        "6 12.3 Север 2024-11-23 15:40:32\r\n" +
        "7 6.8 Север 2024-11-23 15:50:31");

    // Act
    using (StreamReader reader = new StreamReader("Orders.txt"))
    {
        // Регулярное выражение для валидации каждой строки
        Regex regex = new Regex("^\\d* \\d*(\\.\\d*)? [A-Za-zА-Яа-я0-9_]* \\d\\d\\d\\d-[0-1]?\\d-[0-3]?\\d [0-2]?\\d:[0-5]?\\d:[0-5]?\\d$");

        string? line;

        while ((line = reader.ReadLine()) != null)
        {
            if (regex.IsMatch(line.Trim()))
            {
                lines.Add(line);
            }
        }
    }
    List<string> result = new List<string> { "1 15.33333 Север 2024-11-23 15:33:32",
        "2 4 Юг 2024-10-24 20:10:05",
        "3 8 Запад 2024-10-24 20:10:05",
        "4 3.2 Восток 2024-10-24 20:10:05",
        "5 11 Центр 2024-10-24 20:10:05",
        "6 12.3 Север 2024-11-23 15:40:32",
        "7 6.8 Север 2024-11-23 15:50:31" };

    // Assert
    CollectionAssert.AreEqual(result, lines);

    if (File.Exists(configFilePath))
        File.Delete(configFilePath);
}
```
### Результат Unit Tests
![image](https://github.com/user-attachments/assets/7bf1d294-09f6-43cd-940a-29a608a4a5c9)
