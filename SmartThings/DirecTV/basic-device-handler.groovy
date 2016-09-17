/**
 *  DirecTV
 *     Works on DirecTV DVRs
 *
 */

preferences {
	input("destIp", "text", title: "IP", description: "The device IP",required:true)
	input("destPort", "number", title: "Port", description: "The port you wish to connect", required:true)
}
 

metadata {
	definition (name: "MoSoftDirecTV", namespace: "MoSoft", 
    	author: "john@morse-family.org") {
        capability "Actuator"
		capability "Switch"
      	}

	simulator {
		// TODO-: define status and reply messages here
	}

	tiles(scale: 2) {
		standardTile("switch", "device.switch", width: 2, height: 2, canChangeIcon: true, canChangeBackground: true) {
            state "on", label: '${name}', action:"switch.off", backgroundColor: "#79b821", icon:"st.Electronics.electronics18"
            state "off", label: '${name}', action:"switch.on", backgroundColor: "#ffffff", icon:"st.Electronics.electronics18"
        }
		standardTile("guide", "device.switch", width: 1, height: 1, canChangeIcon: false, canChangeBackground: false, inactiveLabel: true) {
            state "guideshown", label: '${name}', action:"guide", backgroundColor: "#79b821", icon:"st.Electronics.electronics13"
            state "guidehidden", label: '${name}', action:"exit", backgroundColor: "#ffffff", icon:"st.Electronics.electronics13"
        }

        main "switch"
        details("switch", "guide")
	}
}

def installed() {
    log.debug "DirecTV device"
    log.debug "installed : device : ${device} hub : ${device.hub}"

    if (device.hub)
    {
        def ip = device.hub.getDataValue("localIP");
        log.debug "device.hub.ip = ${ip}"
        def port = device.hub.getDataValue("localSrvPortTCP")
        log.debug "device.hub.port = ${port}"
    }
	
    def hub = location.hubs[0]

    log.debug "id: ${hub.id}"
    log.debug "zigbeeId: ${hub.zigbeeId}"
    log.debug "zigbeeEui: ${hub.zigbeeEui}"

    // PHYSICAL or VIRTUAL
    log.debug "type: ${hub.type}"

    log.debug "name: ${hub.name}"
    log.debug "firmwareVersionString: ${hub.firmwareVersionString}"
    log.debug "localIP: ${hub.localIP}"
    log.debug "localSrvPortTCP: ${hub.localSrvPortTCP}"
}

//  There is no real parser for this device
// ST cannot interpret the raw packet return, and thus we cannot 
//  do anything with the return data.  
//  http://community.smartthings.com/t/raw-tcp-socket-communications-with-sendhubcommand/4710/10
//
def parse(String description) {
	log.debug "Parsing '${description}'"
}

def on() {
	sendEvent(name: "switch", value: 'on')
	//request("/remote/processKey?key=poweron&hold=keyPress")
	sendKeyPress("poweron")
}

def off() {
	sendEvent(name: "switch", value: 'off')
	//request("/remote/processKey?key=poweroff&hold=keyPress")
	sendKeyPress("poweroff")
}

def exit() { 
	sendEvent(name: "list", value: "listhidden")
	sendEvent(name: "guide", value: "guidehidden")
	sendKeyPress("exit")
}

def list() {
	sendEvent(name: "list", value: 'listshown')
	sendKeyPress("list")
}

def guide() {
	sendEvent(name: "guide", value: 'guideshown')
	sendKeyPress("guide")
}

def sendKeyPress(key) {
    def requestString = "/remote/processKey?key=${key}&hold=keyPress"
    log.debug "sendKeyPress('${requestString}')"
    request(requestString)
}

def request(body) { 
    //def hubAction = new physicalgraph.device.HubAction(
   	// 		'method' : 'GET',
    //		'path' : "/remote/processKey?key=guide&hold=keyPress HTTP/1.1\r\n\r\n",
    //    	//'body': body,
    //    	'headers' : [ HOST: "$destIp:$destPort" ]
	//	) 

    def hubAction = new physicalgraph.device.HubAction(
   	 		'method' : 'GET',
    		'path' : "${body} HTTP/1.1\r\n\r\n",
        	'headers' : [ HOST: "$destIp:$destPort" ]
		) 

    return hubAction
}
