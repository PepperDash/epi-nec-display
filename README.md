# NEC Display Plugin

This is a plugin repo for NEC Displays.

For API documentation, see documents folder.

## RS232 Specification

| Property     | Value |
| ------------ | ----- |
| Baudrate     | 9,600 |
| Data Bits    | 8     |
| Parity       | N/A   |
| Start Bits   | 1     |
| Stop Bits    | 1     |
| Flow Control | N/A   |

### Essentials RS232 Device Configuration

```json
{
    "key": "display-1",
    "name": "Display",
    "type": "necmpsx",
    "group": "display",
    "properties": {
        "control": {
            "method": "com",
            "controlPortDevKey": "processor",
            "controlPortNumber": 1,
            "comParams": {
                "protocol": "RS232",
                "baudRate": 9600,
                "dataBits": 8,
                "stopBits": 1,
                "parity": "None",
                "softwareHandshake": "None",
                "hardwareHandshake": "None",
                "pacing": 0
            }
        },
        "friendlyNames": [                  //if you want to use friendly names, add this section
	        {
	        	"inputKey": "90",           //The input key for the input you want to use a friendly name for, this has to a valid inputkey(DP1, HDMI1, HDMI2)
	        	"name": "Friendly Name 1",  //The desired name to be displayed on the screen
            "hideInput": false              //if set to true, the input will not be displayed in the list of inputs
	        },
	        {
	        	"inputKey": "91",
	        	"name": "Friendly Name 2",
            "hideInput": false
	        },
	        {
	        	"inputKey": "c0",
	        	"name": "Friendly Name 3",
            "hideInput": true

	        }
        ],
        "id": "\x2A",
    }
}
```
<!-- START Minimum Essentials Framework Versions -->
### Minimum Essentials Framework Versions

- 1.16.0
<!-- END Minimum Essentials Framework Versions -->
<!-- START Config Example -->
### Config Example

```json
{
    "key": "GeneratedKey",
    "uid": 1,
    "name": "GeneratedName",
    "type": "NecDisplayConfig",
    "group": "Group",
    "properties": {
        "Id": "SampleString",
        "WarmupTime": "SampleValue",
        "CooldownTime": "SampleValue",
        "friendlyNames": [
            {
                "inputKey": "SampleString",
                "name": "SampleString",
                "hideInput": true
            }
        ]
    }
}
```
<!-- END Config Example -->
<!-- START Supported Types -->

<!-- END Supported Types -->
<!-- START Join Maps -->

<!-- END Join Maps -->
<!-- START Interfaces Implemented -->
### Interfaces Implemented

- IBasicVolumeWithFeedback
- ICommunicationMonitor
- IBridgeAdvanced
- IHasInputs<string>
- ISelectableItems<string>
<!-- END Interfaces Implemented -->
<!-- START Base Classes -->
### Base Classes

- TwoWayDisplayBase
- JoinMapBaseAdvanced
//
<!-- END Base Classes -->
<!-- START Public Methods -->
### Public Methods

- public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
- public void Poll()
- public void AppendChecksumAndSend(string s)
- public void PictureMuteOn()
- public void PictureMuteOff()
- public void PictureMuteToggle()
- public void MatrixModeOn()
- public void MatrixModeOff()
- public void InputHdmi1()
- public void InputHdmi2()
- public void InputHdmi3()
- public void InputHdmi4()
- public void InputDisplayPort1()
- public void InputDisplayPort2()
- public void InputDvi1()
- public void InputVideo1()
- public void InputVga()
- public void InputRgb()
- public void MuteOff()
- public void MuteOn()
- public void MuteToggle()
- public void VolumeDown(bool pressRelease)
- public void VolumeUp(bool pressRelease)
- public void Select()
<!-- END Public Methods -->
<!-- START Bool Feedbacks -->
### Bool Feedbacks

- VideoIsMutedFeedback
- MuteFeedback
<!-- END Bool Feedbacks -->
<!-- START Int Feedbacks -->
### Int Feedbacks

- VolumeLevelFeedback
<!-- END Int Feedbacks -->
<!-- START String Feedbacks -->

<!-- END String Feedbacks -->
