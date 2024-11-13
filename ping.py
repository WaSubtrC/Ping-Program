import socket
import struct
import time
import sys

def calculate_checksum(packet):
    """Calculate the ICMP checksum."""
    if len(packet) % 2 != 0:
        packet += b'\0'
    checksum = 0
    for i in range(0, len(packet), 2):
        checksum += (packet[i] << 8) + (packet[i + 1])
    checksum = (checksum >> 16) + (checksum & 0xffff)
    checksum = ~checksum & 0xffff
    return checksum

def send_ping(destination, count=4, data_size=64):
    """Send ICMP echo request packets to the destination."""
    with socket.socket(socket.AF_INET, socket.SOCK_RAW, socket.IPPROTO_ICMP) as sock:
        sock.settimeout(2.0)

        seq_number = 0
        sent = 0
        received = 0
        min_time = float('inf')
        max_time = 0
        total_time = 0

        for _ in range(count):
            # Construct ICMP header
            header = struct.pack('!BBHL', 8, 0, 0, seq_number)
            data = b'x' * data_size
            packet = header + data
            checksum = calculate_checksum(packet)
            header = struct.pack('!BBHL', 8, 0, checksum, seq_number)
            packet = header + data

            try:
                send_time = time.time()
                sock.sendto(packet, (destination, 0))
                sent += 1

                reply, addr = sock.recvfrom(1024)
                receive_time = time.time()
                elapsed = (receive_time - send_time) * 1000

                min_time = min(min_time, elapsed)
                max_time = max(max_time, elapsed)
                total_time += elapsed
                received += 1

                print(f'Reply from {addr[0]}: bytes={len(reply)} time={elapsed:.2f}ms')
            except socket.timeout:
                print(f'Request timed out for sequence number {seq_number}')

            seq_number += 1

        print(f'\nPing statistics for {destination}:')
        print(f'    Packets: Sent = {sent}, Received = {received}, Lost = {sent - received} ({(sent - received) / sent * 100:.2f}% loss)')
        print(f'    Approximate round trip times in milli-seconds:')
        print(f'    Minimum = {min_time:.2f}ms, Maximum = {max_time:.2f}ms, Average = {total_time / received:.2f}ms')
#send_ping('127.0.0.1')   
send_ping('8.8.8.8', count=10, data_size=128)