# ProjecteDronVR
Projecte per controlar un dron DJITello amb un script de python i connectar-ho a unity per poder-ho manejar íntegrament desde allà.

<img src="https://storage.ghost.io/c/66/89/6689e62f-4e74-478c-9d84-2eac176b3d5d/content/images/2025/07/b01_tello-livo_dji_opt.png" width="300">

Per poder connectar el dron a unity, primer s'ha d'executar el script de python, que farà de servidor i connexió per lo que asignarà un port, que escoltarà unity, I un port de video, per mostrar la camera desde unity.

A unity executarem el temps de joc, I podrem observar i controlar el dron. 
## Controls:

- Tecla T = Enlairar-se

- Tecla L = Aterrar

- W+A+S+D = Control de moviment

- Fletxeta Esquerre = rotar esquerre

- Fletxeta Dreta = rotar dreta

- Fletxeta Amunt = pujar altura

- Fletxeta Avall = baixar altura


Cal dir que hi ha dues escenes la escena inicial que esta pensada per controlar desde ordinador, i hi ha una alter escena per fer proves amb el dron amb realitat virtual (encara que aquest darrer no esta acabat)
