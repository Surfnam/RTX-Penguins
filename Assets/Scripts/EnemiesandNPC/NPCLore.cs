
public static class NPCLore
{
    public static string GetDialogue(int number)
    {
        switch (number)
        {
            case 1:
                return "Hi friend. It's a lovely day isn't it? I was just waddling around the glaciers when I noticed something strange over there. Things have been off recently... Some of my friends have been looking and acting differently. Can you check it out for me?";

            case 2:
                return "AHH! Oh, it's you. I'm sorry to be rude, but you scared me a bit. The other day I went to Frenguin's house to have some fish with him, and something started shooting at me. Ever since then, I've been on edge. If you notice something strange, be careful, okay?";

            case 3:
                return "Hey, dude. Have you noticed that some of our friends look more red than usual? And they're walking in a sort of mechanical way. I'm scared to approach some of them, because Grenguin told me that something happened to him when he interacted with one the other day. If you see a friend that looks more red than usual then try to avoid them.";

            case 4:
                return "Oh god, everything feels all wrong recently. I've been noticing these strange metal machines appearing all over the place. I saw Kenguin assemble one the other night, when I was out for a little moonlight swim. It's just, he seemed different than usual. Something is going on.";

            case 5:
                return "I can't talk now, I've lost something important. I was on my way home from the fish market when this strange metal device starting shooting at me. I lost my precious pebble, and I need to find it before I go home tonight. Let's talk another time.";

            case 6:
                return "Hi there! The weather has been nice and warm recently hasn't it? Although it's a bit strange, I don't think that it should be this warm. I mean, it never has been before. Do you think it's related to those horrible metal machines that I've seen all over the place?";

            case 7:
                return "Oh, my dear friend. How lovely to see you. I wish I could talk more but I am in a bit of a hurry. I promised that I would get back home soon, but I can't seem to find a good path. My normal glacial route has melted, and you know the water is dangerous nowadays. I just need to practice my jumps, I guess. Talk soon!";

            case 8:
                return "Oh dear, oh goodness. I don't know what to do! Have you noticed how hot it has been recently? Ever since those metal things started appearing, the ice is melting and I've even seen fire appear! The world seems quite dangerous these days.";
            
            default:
                return GetDialogue(1);
        }
    }
}

