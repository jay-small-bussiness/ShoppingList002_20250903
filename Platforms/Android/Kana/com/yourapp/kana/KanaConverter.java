package com.yourapp.kana;

import java.io.InputStream;
import java.io.*;
import java.io.ByteArrayInputStream;
import java.util.List;
import com.atilika.kuromoji.ipadic.Token;
import com.atilika.kuromoji.ipadic.Tokenizer;
import java.nio.charset.StandardCharsets; 
import java.util.Arrays;

public class KanaConverter {
    private static Tokenizer tokenizer;
	private static String userDictContent;  // 

	public static synchronized void setUserDictionary(String content) {
	    userDictContent = content;
	    rebuildTokenizer();
	}

	private static synchronized void rebuildTokenizer() {
	    System.out.println("DEBUG rebuildTokenizer entered");

	    Tokenizer.Builder b = new Tokenizer.Builder();
	    try {
		    if (userDictContent != null) {
		        BufferedReader br = new BufferedReader(
		            new InputStreamReader(
		                new ByteArrayInputStream(userDictContent.getBytes(StandardCharsets.UTF_8)),
		                StandardCharsets.UTF_8));

		        String line;
		        int lineNo = 1;
		        while ((line = br.readLine()) != null) {
			        line = line.trim();
		            String[] cols = line.split(",");
		            //System.out.println("DEBUG userdict line[" + lineNo + "] = " + line + " / cols=" + cols.length);
		            //System.out.println("DEBUG userdict line[" + lineNo + "] raw=" + Arrays.toString(line.getBytes(StandardCharsets.UTF_8)));
					System.out.println("DEBUG userdict line[" + lineNo + "] = " + line + " / cols=" + cols.length);

		            lineNo++;
		        }
		        br.close();

		        InputStream is = new ByteArrayInputStream(userDictContent.getBytes(StandardCharsets.UTF_8));
		        b.userDictionary(is);
		        System.out.println("DEBUG userdict loaded (content mode)");
		    }
		} catch (Exception ex) {
		    System.out.println("UserDict load failed: " + ex);
		    ex.printStackTrace();
		}
	    tokenizer = b.build();
	}

    private static Tokenizer ensureTokenizer() {
        if (tokenizer == null) {
            tokenizer = new Tokenizer.Builder().build();
        }
        return tokenizer;
    }

    public static String getReadingKatakana(String text) {
	    try {
	        BufferedReader br = new BufferedReader(new InputStreamReader(
	            new ByteArrayInputStream(text.getBytes(StandardCharsets.UTF_8)),
	            StandardCharsets.UTF_8));

	        String line;
	        while ((line = br.readLine()) != null) {
	            String[] cols = line.split(",");
	            System.out.println("DEBUG userdict line=" + line + " / cols=" + cols.length);
	        }
	        br.close();  
	    } catch (Exception e) {
	        System.out.println("DEBUG error while reading userdict content: " + e);
	        e.printStackTrace();
	    }

	    try {
	        System.out.println("DEBUG getReadingKatakana start: " + text);
	        List<Token> tokens = ensureTokenizer().tokenize(text);
	        StringBuilder sb = new StringBuilder();
	        for (Token t : tokens) {
	            String reading = t.getReading();
				if (reading == null || reading.equals("*")) {
					reading = t.getSurface();  
				}
				System.out.println("DEBUG TOKEN Surface=" + t.getSurface() + ", Reading=" + reading);
				sb.append(reading);
	        }
	        return sb.toString();
	    } catch (Exception e) {
	        System.out.println("JAVA EX in getReadingKatakana: " + e);
	        e.printStackTrace();
	        return "JAVA_ERROR";
	    }
	}

}

