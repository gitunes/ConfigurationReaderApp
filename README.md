# ConfigurationReaderApp

Bu uygulamada .NET CORE 6 sürümü kullanılmıştır.\
ConfigurationApp, appconfig veya webconfin'in yanı sıra dinamik olarak ayarlarınızı tutabileceğiniz bir projedir. Verilen süreye göre kendini tekrar çalıştıran bir yapıya sahiptir.

## Yapılması gerekenler
1-ConfigurationApp içindeki docker-compose.yml dosyası ile redis serverini localhostta kurmak (docker-compose -f docker-compose.yml up).\
2-ConfigurationApp projesini build aldıktan sonra oluşan dll dosyasını projenize referans göstermek.\
3-Projeninizde bulunan bin/debug/net6.0 dizininin içine records.db dosyasını atmak.\
Bu adımlardan sonra ConfigurationApp 'i 
>new ConfigurationReader(_connectionString, _applicationName, _refreshTimerIntervalInMs); \
şeklinde çağırıp oluşturduğunuz nesne ile GetValue< T >(string key) methodunu kullanabilirsiniz.
Bu method önce cacheden datayı almaya çalışacaktır. Eğer data cachede yok ise dbden getirecektir.
  
## Test Çalışmaları
ConfigurationTestUnit projesi içinde kullanabileceğiniz 3 adet test methodu yazılmıştır.
