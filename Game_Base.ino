
  #include <SPI.h>
  #include <RF24.h>
  #include <Wire.h>
  #include <math.h> 

  #define BASE_ID 3
  #define RF_CHANNEL 108
  #define NRF_CE_PIN 6
  #define NRF_CSN_PIN 7
  #define TX_INTERVAL 500

  uint8_t pipeNum;
  const byte addrA[] = "1ABME";
  const byte addrB[] = "2ABME";

  struct __attribute__((packed)) SensorPacket{ 
  uint8_t nodeId;
  float roll, pitch, yaw; 
  int c = 0;
  };

  String buildJson (SensorPacket &p);

  RF24 radio(NRF_CE_PIN, NRF_CSN_PIN);

  void setup() {
    // put your setup code here, to run once:
    Serial.begin(115200);

    if(!radio.begin()){
      Serial.println(F("NRF Error!!"));
    }

    radio.setChannel(RF_CHANNEL);
    radio.setDataRate(RF24_250KBPS);
    radio.setPALevel(RF24_PA_MAX);
    radio.setRetries(15, 15);
    radio.setPayloadSize(sizeof(SensorPacket));

    radio.openReadingPipe(1, addrA);
    radio.openReadingPipe(2, addrB);
    radio.startListening();
  }

    String buildJson(SensorPacket &p) {
    String s = "{\"id\":";
    s += p.nodeId;
    s += ",\"roll\":";
    s += String(p.roll, 6);
    s += ",\"pitch\":";
    s += String(p.pitch, 6);
    s += ",\"yaw\":";
    s += String(p.yaw, 6);
    s += "}";
    return s;
  }

  void loop() {
    // put your main code here, to run repeatedly:
    if (radio.available(&pipeNum)){
      SensorPacket pkt;
      radio.read(&pkt, sizeof(pkt));
      Serial.println(buildJson(pkt));
    }
  }


