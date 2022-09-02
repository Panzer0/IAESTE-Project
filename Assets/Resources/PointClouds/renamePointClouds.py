import os
 
# Function to rename multiple files
def main():
    print("Start")
    for pc in ["longdress", "loot", "redandblack", "soldier"]:
        src = f"{pc}/"
        print(pc)
        for count, filename in enumerate(os.listdir(src)):
            print(filename)           
            f = f"{pc}_{(count+1):04}.ply"
            path = src + f
            os.rename(src + filename , path)

 
# Driver Code
if __name__ == '__main__':
     
    # Calling main() function
    main()