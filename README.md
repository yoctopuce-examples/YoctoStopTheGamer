# YoctoStopTheGamer

A Windows service that watch the state of a remote button connected on a Yoctopuce device and play a message if the button is pressed.
Read our full article on our web site:

## Usage

````
Usage:
  YoctoStopTheGamer --install <URL> <HwId> [Opt] : Install the service
  YoctoStopTheGamer --uninstall                  : Uninstall the service
  YoctoStopTheGamer --test <URL> <HwId> [Opt]    : Test service without installing it
Options:
  --msg <value>    : The message to read if the button is pressed
                     Default value is "Stop playing"
  --locale <value> : The locale of text speech to use
  --help           : Help message
````

## Examples

### Test without installing the service

The following code allow you to test the application and the parameters form the command line without installing the service.
It will watch the input ``anButton1`` of the [Yocto-Knob](https://www.yoctopuce.com/EN/products/usb-electrical-sensors/yocto-knob) that
is connected on a YoctoHub that has the IP of ``192.168.1.77``. The application will read the message ``Text to read`` on the speaker
using Windows text to speech functionality.

````
YoctoStopTheGamer.exe --test 192.168.1.77 YBUTTON1-1234.anButton1 --msg "Text to read" --locale en
````


### Install as service

To install the application as a service you just need to replace ``--test`` by ``--install`` and run it form a terminal that has administrator
rights. The service will be listed as "Gamer Compliance Service".

````
YoctoStopTheGamer.exe --install 192.168.1.77 YBUTTON1-1234.anButton1 --msg "Text to read" --locale en
````



### Uninstall as service

To remove the service use the ``--uninstall`` argument  form a terminal that has administrator
rights.

````
YoctoStopTheGamer.exe --uninstall
````

