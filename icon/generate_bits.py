#!/bin/python
# generate_bits.py: Create a bit pattern.
#
# Copyright (c) 2016 Benito Palacios Sánchez
#
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:
#
# The above copyright notice and this permission notice shall be included in all
# copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
# SOFTWARE.
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
