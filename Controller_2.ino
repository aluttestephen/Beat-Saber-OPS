
  #include <SPI.h>
  #include <RF24.h>
  #include <Wire.h>
  #include <math.h> 

  

  #define CONTROLLER_ID  2
  #define RF_CHANNEL 108
  #define TX_INTERVAL 500
  #define NRF_CE_PIN 6
  #define NRF_CSN_PIN 7
  #define IMU_ADDR 0x68

  const byte pipeAddr[] = "2ABME";

  struct __attribute__((packed)) SensorPacket{ 
    uint8_t nodeId;
    float roll, pitch, yaw; 
    int c = 0;
  };

  const int IMU = 0x68;
  float AccX, AccY, AccZ;
  float GyroX, GyroY, GyroZ;
  float accAngleX, accAngleY, gyroAngleX, gyroAngleY, gyroAngleZ; 
  float gyroXoffset, gyroYoffset, gyroZoffset;
  float elapsedTime, currentTime, previousTime; 
  RF24 radio(NRF_CE_PIN, NRF_CSN_PIN);

  SensorPacket packet;
  
  void calibrateGyro(){
    Serial.println(F("Calibrating - Hold in place..."));
    delay(10);
    long sumX = 0, sumY = 0, sumZ = 0;
    int samples = 2000;

    for (int i = 0; i < samples; i++){
      Wire.beginTransmission(IMU_ADDR);
      Wire.write(0x43);
      Wire.endTransmission(false);
      Wire.requestFrom(IMU_ADDR, 6, true);

      sumX += (Wire.read() << 8| Wire.read());
      sumY += (Wire.read() << 8| Wire.read());
      sumZ += (Wire.read() << 8| Wire.read());
    }

    gyroXoffset = sumX / (samples * 131.0);
    gyroYoffset = sumY / (samples * 131.0);
    gyroZoffset = sumZ / (samples * 131.0);
    Serial.println(F("Calibration done."));
  }

  void setup() {
    Serial.begin(115200);
    Serial.println(F("Constroller 2 Starting"));
    Wire.begin();

    Wire.beginTransmission(IMU_ADDR);
    Wire.write(0x6B); 
    Wire.write(0X00);
    Wire.endTransmission(true);

    if (!radio.begin()) {
      Serial.println(F("ERROR: NRF FAIL!"));
    } 

    radio.setChannel(RF_CHANNEL);
    radio.setDataRate(RF24_250KBPS);
    radio.setPALevel(RF24_PA_MAX);
    radio.setRetries(15,15);
    radio.setPayloadSize(sizeof(SensorPacket));
    radio.openWritingPipe(pipeAddr);
    radio.stopListening();

    Serial.println(F("Controller 2 Ready!!"));
    packet.nodeId = CONTROLLER_ID;
    calibrateGyro();
    previousTime = millis();
  };


  void loop() {

    static long lastSendTime = 0;

    // Accelerometer 
    Wire.beginTransmission(IMU_ADDR);
    Wire.write(0x3B);
    Wire.endTransmission(false);
    Wire.requestFrom(IMU_ADDR, 6, true);

    AccX = (Wire.read() << 8| Wire.read()) / 16384.0;
    AccY = (Wire.read() << 8| Wire.read()) / 16384.0;
    AccZ = (Wire.read() << 8| Wire.read()) / 16384.0;

    accAngleX = (atan(AccY/ sqrt(pow(AccX, 2) + pow(AccZ, 2))) * 180/PI) - 0.58; 
    accAngleY = (atan(-1*AccX/ sqrt(pow(AccY,2) + pow(AccZ,2))) *180/PI) + 1.58;

    
    //Gyroscope

    previousTime = currentTime;        
    currentTime = millis();           
    elapsedTime = (currentTime - previousTime) / 1000; 
    Wire.beginTransmission(IMU_ADDR);
    Wire.write(0x43); 
    Wire.endTransmission(false);
    Wire.requestFrom(IMU_ADDR, 6, true); 
    GyroX = (Wire.read() << 8 | Wire.read()) / 131.0; 
    GyroY = (Wire.read() << 8 | Wire.read()) / 131.0;
    GyroZ = (Wire.read() << 8 | Wire.read()) / 131.0;
    
    GyroX -= gyroXoffset; 
    GyroY -= gyroYoffset; 
    GyroZ -= gyroZoffset;

    float deadzone = 0.03;

    if (abs(GyroX) < deadzone) GyroX = 0;
    if (abs(GyroY) < deadzone) GyroY = 0;
    if (abs(GyroZ) < deadzone) GyroZ = 0;
   
    gyroAngleX = gyroAngleX + GyroX * elapsedTime; 
    gyroAngleY = gyroAngleY + GyroY * elapsedTime;
    packet.yaw =  packet.yaw + GyroZ * elapsedTime;
    
    float gyroMagnitude = sqrt(pow(GyroX,2) + pow(GyroY,2) + pow(GyroZ,2));
    float alpha = (gyroMagnitude > 0.5) ? 0.96 : 0.50;

    packet.roll = alpha * gyroAngleX + (1.0 - alpha) * accAngleX;
    packet.pitch = alpha * gyroAngleY + (1.0 - alpha) * accAngleY;

    if (millis() - lastSendTime >= TX_INTERVAL) {
      radio.write(&packet, sizeof(packet));
      lastSendTime = millis() + random(0,10);

      Serial.print(F("Roll: "));  Serial.print(packet.roll);
      Serial.print(F(" | Pitch: ")); Serial.print(packet.pitch);
      Serial.print(F(" | Yaw: "));   Serial.println(packet.yaw);
    }
  }
