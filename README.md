# Примечание
Полный список заказов находится в файле *Orders.txt*. В него можно добавлять или удалять записи. В файле *config.txt* находятся входные параметры, на основе которых будет происходить фильтрация. Их также можно изменять для различных результатов.
 
## 1. Запуск приложения

### 1.1
 **а)** Для запуска приложения через консоль, необходимо перейти в консоли в папку с исполняемым файлом **(bin/Release/net8.0)** и написать "*EffectiveMobileTestTask.exe название_района yyyy-mm-dd hh:mm:ss*" (без кавычек).

 **б)** Если не указать параметры через консоль или запустить приложение двойным кликом по исполняемому файлу **(bin/Release/net8.0/EffectiveMobileTestTask.exe)**, то параметры будут взяты автоматически из конфигурационного файла config.txt в той же папке.

## 2. Просмотр результатов и логов

### 2.1
После запуска приложения в консоль будет выведена информация, которая будет продублирована в лог файл (*_deliveryLog.txt*). Результаты работы программы будут находится в файле _deliveryOrders.txt.