#!/bin/python
"""
    generate_bits.py: Create a bit pattern.
    Copyright (C) 2014 pleonex
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 2 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.
    
    You should have received a copy of the GNU General Public License
    along with this program. If not, see <http://www.gnu.org/licenses/>.
""" 
import random
import math


def showRandom(n):
    bits = []
    for i in range(n):
        number = random.randint(0, 255)
        for b in range(8):
            bits.append("{0}".format((number >> b) & 1))
    return bits


def printBits(max_line, lines):
    nums = int(math.ceil(lines * max_line / 8 + 0.1))
    print nums
    bits = "".join(showRandom(nums))
    for i in range(0, len(bits), max_line):
        stop = i + max_line
        if stop > len(bits):
            stop = len(bits)

        print bits[i:stop]

printBits(42, 22)
