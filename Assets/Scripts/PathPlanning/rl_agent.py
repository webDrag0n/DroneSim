import socket
import time
import rrt_star3D

p = rrt_star3D.rrtstar()

def main():
    # 1. 创建tcp的套接字
    tcp_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    
    try:
        # 2. 链接服务器
        server_ip = '127.0.0.1'
        server_port = 12578
        server_addr = (server_ip, server_port)
        tcp_socket.connect(server_addr)
        
        # client端必须先发送数据以让server端准备好
        send_data = 'start'
        tcp_socket.send(send_data.encode("utf-8"))
        # 3. 发送数据/接收数据
        while True:
            # 接收数据
            recv_data = tcp_socket.recv(1024).decode('utf-8')
            if not recv_data:
                break
            if recv_data == 'block':
                send_data = str(p.env.blocks.tolist())
                # send_data = 'block'
            elif recv_data == 'ball':
                send_data = str(p.env.balls.tolist())
                # send_data = 'ball'
            elif recv_data == 'obb':
                obb_list = [[5.0,7.0,2.5,0.5,2.0,2.5,135,0,0], [12.0,4.0,2.5,0.5,2.0,2.5,45,0,0]]
                send_data = str(obb_list)
            elif recv_data == 'point':
                point_list = [list(p.x0), list(p.xt)]
                send_data = str(point_list)
            elif recv_data == 'path':
                p.run()
                arr_list = [arr.tolist() for arr in p.Path]
                # arr_list = [[[6.0, 16.0, 0.0], [5.543133159825267, 15.472094279664898, 0.11397049259382785]], [[5.543133159825267, 15.472094279664898, 0.11397049259382785], [5.393283602903396, 17.14761228791091, 0.5332292835218151]]]
                send_data = str(len(arr_list))
                tcp_socket.send(send_data.encode("utf-8"))
                time.sleep(0.01)
                for item in arr_list:
                    send_data = str(item)
                    tcp_socket.send(send_data.encode("utf-8"))
                    time.sleep(0.01)
                send_data = 'OK'
                # send_data = 'path'
            else:
                x, y, z = map(float, recv_data.split(','))
                sum_result = x + y + z
                send_data = str(sum_result)
            tcp_socket.send(send_data.encode("utf-8"))

            # 加入延时，避免过于频繁的数据交换
            # time.sleep(0.1)  # 延时0.1秒
    except Exception as e:
        print("Error:", e)
    finally:
        # 4. 关闭套接字
        tcp_socket.close()

if __name__ == "__main__":
    # p = rrt_star3D.rrtstar()
    # starttime = time.time()
    # p.run()
    main()