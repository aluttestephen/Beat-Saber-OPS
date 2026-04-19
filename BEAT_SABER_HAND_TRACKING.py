import mediapipe as mp
import socket
import json 
import math
import cv2

UDP_IP = "127.0.0.1"
UDP_PORT = 5005
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
ALPHA = 0.7
SWING_THRESHOLD = 0.015


mp_hands = mp.solutions.hands
mp_draw = mp.solutions.drawing_utils
cap = cv2.VideoCapture (0)

class HandState: 
    def __init__(self):
        self.x = self.y = 0.5
        self.vel_x = self.vel_y = 0
        self.dir_x = self.dir_y = 0

    def update (self, wrist, mid_mcp):
        new_x = ALPHA * wrist.x + (1-ALPHA) * self.x
        new_y = ALPHA * wrist.y + (1-ALPHA) * self.y

        self.vel_x = new_x -self.x
        self.vel_y = new_y - self.y

        self.x, self.y = new_x, new_y

        self.dir_x = mid_mcp.x - wrist.x
        self.dir_y = mid_mcp.y - wrist.y

    @property
    def swing_speed(self):
        return (self.vel_x**2 + self.vel_y**2) **0.5
    
    @property
    def swing_angle_deg(self):
        return math.degrees(math.atan2(-self.vel_y, self.vel_x))
    
player1 = HandState()
player2 = HandState()

def send_to_unity(hand_id, x,y,z):
    payload = json.dumps({"id": hand_id, "x": x, "y": y, "z": z})
    sock.sendto(payload.encode(), (UDP_IP, UDP_PORT))

with mp_hands.Hands(max_num_hands = 2, 
                    min_detection_confidence = 0.7,
                      min_tracking_confidence = 0.7) as hands:
    while cap.isOpened():
        ret, frame = cap.read()
        if not ret:
            break
        
        frame = cv2.flip(frame, 1)
        rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        results = hands.process(rgb)

        if results.multi_hand_landmarks:
            for hand_landmarks in results.multi_hand_landmarks:
                wrist = hand_landmarks.landmark[0]
                mid_mcp = hand_landmarks.landmark[9]
                x,y,z = wrist.x, wrist.y, wrist.z


                if wrist.x <0.5:
                    player1.update(wrist, mid_mcp)
                    state = player1
                    player_id = 1

                else:
                    player2.update (wrist, mid_mcp)
                    state = player2
                    player_id = 2

                payload = {
                    "id" : player_id,
                    "x" : round (state.x, 4),
                    "y" : round(state.y, 4),
                    "vel" : round(state.swing_speed, 4),
                    "angle" : round(state.swing_angle_deg, 1),
                    "dir_x" : round(state.dir_x, 4),
                    "dir_y" : round(state.dir_y, 4),
                    "swing" : state.swing_speed > SWING_THRESHOLD
                }

                mp_draw.draw_landmarks(frame, hand_landmarks, mp_hands.HAND_CONNECTIONS)

                h,w, _ = frame.shape

                cv2.line(frame, (w//2, 0), (w//2, h), (0,255,0), 2)
                cv2.putText(frame, "P1", (w//4, 30), cv2.FONT_HERSHEY_SIMPLEX, 1, (0,255,0), 2)
                cv2.putText(frame, "P2", (3*w//4, 30), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255,0), 2)

                cv2.imshow("Hand Tracking", frame)
                if cv2.waitKey(1) & 0xFF == ord('q'):
                    break

                send_to_unity(0, wrist.x, wrist.y, wrist.z)

cap.release()
cv2.destroyAllWindows()


    